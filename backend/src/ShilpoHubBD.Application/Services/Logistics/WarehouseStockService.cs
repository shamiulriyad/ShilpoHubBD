using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

/// <summary>
/// Stock holdings and the append-only movement ledger inside a partner's warehouses: receive, issue,
/// bin-to-bin transfer, adjust / stock-count, reserve and release. Every mutating call writes one or
/// more <see cref="WarehouseStockMovement"/> rows and keeps bin / warehouse occupancy roll-ups in
/// sync in a single save.
/// </summary>
public class WarehouseStockService : IWarehouseStockService
{
    private const int RecentMovementsOnItem = 50;

    private readonly IWarehouseStockRepository _repository;
    private readonly ILogisticsPartnerRepository _partnerRepository;

    public WarehouseStockService(
        IWarehouseStockRepository repository, ILogisticsPartnerRepository partnerRepository)
    {
        _repository = repository;
        _partnerRepository = partnerRepository;
    }

    public async Task<WarehouseStockItemDto> ReceiveAsync(
        Guid currentUserId, bool isAdmin, ReceiveStockRequest request, CancellationToken cancellationToken)
    {
        var profileId = await ResolveProfileIdAsync(currentUserId, isAdmin, cancellationToken);

        var warehouse = await _repository.GetWarehouseAsync(request.WarehouseId, cancellationToken)
            ?? throw new ConflictException("Warehouse not found.");
        EnsureWarehouseOwned(warehouse, profileId);

        if (request.Quantity <= 0)
        {
            throw new ConflictException("Quantity must be greater than zero.");
        }

        WarehouseBin? bin = null;
        if (request.WarehouseBinId.HasValue)
        {
            bin = await _repository.GetBinAsync(request.WarehouseBinId.Value, cancellationToken)
                ?? throw new ConflictException("Bin not found.");
            if (bin.WarehouseId != warehouse.Id)
            {
                throw new ConflictException("Bin does not belong to this warehouse.");
            }
        }

        if (request.ProductId.HasValue
            && !await _repository.ProductExistsAsync(request.ProductId.Value, cancellationToken))
        {
            throw new ConflictException("Product not found.");
        }

        if (request.OwnerUserId.HasValue
            && !await _repository.UserExistsAsync(request.OwnerUserId.Value, cancellationToken))
        {
            throw new ConflictException("Owner user not found.");
        }

        var now = DateTime.UtcNow;
        var occurredAt = ToUtc(request.OccurredAt) ?? now;
        var sku = request.Sku.Trim();
        var batch = string.IsNullOrWhiteSpace(request.BatchNumber) ? null : request.BatchNumber.Trim();

        WarehouseStockItem item;
        if (request.StockItemId.HasValue)
        {
            item = await _repository.GetStockItemByIdAsync(request.StockItemId.Value, cancellationToken)
                ?? throw new ConflictException("Stock item not found.");
            if (item.WarehouseId != warehouse.Id)
            {
                throw new ConflictException("Stock item belongs to a different warehouse.");
            }
        }
        else
        {
            item = await _repository.FindMatchingStockItemAsync(
                warehouse.Id, request.WarehouseBinId, sku, batch, cancellationToken)
                ?? await CreateStockItemAsync(warehouse.Id, request, sku, batch, now, cancellationToken);
        }

        item.QuantityOnHand += request.Quantity;
        RecomputeAvailable(item);
        item.ReceivedAt ??= occurredAt;
        item.LastMovementAt = occurredAt;
        item.UpdatedAt = now;

        if (bin is not null)
        {
            bin.OccupiedUnits += request.Quantity;
            bin.UpdatedAt = now;
        }

        warehouse.UsedCapacityUnits += request.Quantity;
        warehouse.UpdatedAt = now;

        await AddMovementAsync(warehouse.Id, item, WarehouseStockMovementType.Inbound, request.Quantity,
            currentUserId, occurredAt, now, fromBinId: null, toBinId: request.WarehouseBinId,
            request.ReferenceType, request.ReferenceId, request.Reason, request.Note, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildItemDtoAsync(item.Id, cancellationToken);
    }

    public async Task<PagedResult<WarehouseStockItemListItemDto>> GetStockItemsAsync(
        Guid currentUserId, bool isAdmin, WarehouseStockQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var profileId = await ResolveProfileIdAsync(currentUserId, isAdmin, cancellationToken);
        var (items, totalCount) = await _repository.GetStockItemsPagedAsync(profileId, query, cancellationToken);

        return new PagedResult<WarehouseStockItemListItemDto>
        {
            Items = items.Select(i => i.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<WarehouseStockItemDto> GetStockItemByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        await LoadOwnedItemAsync(currentUserId, isAdmin, id, cancellationToken);
        return await BuildItemDtoAsync(id, cancellationToken);
    }

    public async Task<WarehouseStockItemDto> IssueAsync(
        Guid currentUserId, bool isAdmin, Guid id, IssueStockRequest request, CancellationToken cancellationToken)
    {
        var item = await LoadOwnedItemAsync(currentUserId, isAdmin, id, cancellationToken);

        if (request.Quantity <= 0)
        {
            throw new ConflictException("Quantity must be greater than zero.");
        }

        if (request.Quantity > item.QuantityAvailable)
        {
            throw new ConflictException(
                $"Only {item.QuantityAvailable} unit(s) are available to issue (reserved stock cannot be issued).");
        }

        var now = DateTime.UtcNow;
        var occurredAt = ToUtc(request.OccurredAt) ?? now;

        item.QuantityOnHand -= request.Quantity;
        RecomputeAvailable(item);
        item.LastMovementAt = occurredAt;
        item.UpdatedAt = now;

        ApplyBinDelta(item.Bin, -request.Quantity, now);
        ApplyWarehouseDelta(item.Warehouse, -request.Quantity, now);

        await AddMovementAsync(item.WarehouseId, item, WarehouseStockMovementType.Outbound, request.Quantity,
            currentUserId, occurredAt, now, fromBinId: item.WarehouseBinId, toBinId: null,
            request.ReferenceType, request.ReferenceId, request.Reason, request.Note, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildItemDtoAsync(item.Id, cancellationToken);
    }

    public async Task<WarehouseStockItemDto> TransferAsync(
        Guid currentUserId, bool isAdmin, Guid id, TransferStockRequest request, CancellationToken cancellationToken)
    {
        var item = await LoadOwnedItemAsync(currentUserId, isAdmin, id, cancellationToken);

        if (request.Quantity <= 0)
        {
            throw new ConflictException("Quantity must be greater than zero.");
        }

        if (request.Quantity > item.QuantityOnHand)
        {
            throw new ConflictException($"Only {item.QuantityOnHand} unit(s) are on hand to transfer.");
        }

        if (request.ToBinId == item.WarehouseBinId)
        {
            throw new ConflictException("Source and destination bins are the same.");
        }

        var toBin = await _repository.GetBinAsync(request.ToBinId, cancellationToken)
            ?? throw new ConflictException("Destination bin not found.");
        if (toBin.WarehouseId != item.WarehouseId)
        {
            throw new ConflictException("Destination bin belongs to a different warehouse.");
        }

        var isFullMove = request.Quantity == item.QuantityOnHand;
        if (!isFullMove && request.Quantity > item.QuantityAvailable)
        {
            throw new ConflictException(
                "Only unreserved stock can be split into another bin — release the reservation or transfer the full quantity.");
        }

        var now = DateTime.UtcNow;
        var occurredAt = ToUtc(request.OccurredAt) ?? now;
        var fromBinId = item.WarehouseBinId;

        if (isFullMove)
        {
            // move the whole holding
            ApplyBinDelta(item.Bin, -request.Quantity, now);
            item.WarehouseBinId = toBin.Id;
            item.Bin = toBin;
            toBin.OccupiedUnits += request.Quantity;
            toBin.UpdatedAt = now;
            item.LastMovementAt = occurredAt;
            item.UpdatedAt = now;

            await AddMovementAsync(item.WarehouseId, item, WarehouseStockMovementType.TransferOut, request.Quantity,
                currentUserId, occurredAt, now, fromBinId, toBin.Id, "Transfer", null, request.Note, null, cancellationToken);
            await AddMovementAsync(item.WarehouseId, item, WarehouseStockMovementType.TransferIn, request.Quantity,
                currentUserId, occurredAt, now, fromBinId, toBin.Id, "Transfer", null, request.Note, null, cancellationToken);
        }
        else
        {
            // split off part of the holding into a destination stock item
            item.QuantityOnHand -= request.Quantity;
            RecomputeAvailable(item);
            item.LastMovementAt = occurredAt;
            item.UpdatedAt = now;
            ApplyBinDelta(item.Bin, -request.Quantity, now);

            var dest = await _repository.FindMatchingStockItemAsync(
                item.WarehouseId, toBin.Id, item.Sku, item.BatchNumber, cancellationToken);
            if (dest is null)
            {
                dest = new WarehouseStockItem
                {
                    Id = Guid.NewGuid(),
                    WarehouseId = item.WarehouseId,
                    WarehouseBinId = toBin.Id,
                    ProductId = item.ProductId,
                    OwnerUserId = item.OwnerUserId,
                    Sku = item.Sku,
                    Description = item.Description,
                    UnitOfMeasure = item.UnitOfMeasure,
                    BatchNumber = item.BatchNumber,
                    ExpiryDate = item.ExpiryDate,
                    Status = item.Status,
                    UnitValue = item.UnitValue,
                    ReceivedAt = item.ReceivedAt,
                    CreatedAt = now,
                    UpdatedAt = now,
                };
                await _repository.AddStockItemAsync(dest, cancellationToken);
            }

            dest.QuantityOnHand += request.Quantity;
            RecomputeAvailable(dest);
            dest.LastMovementAt = occurredAt;
            dest.UpdatedAt = now;
            toBin.OccupiedUnits += request.Quantity;
            toBin.UpdatedAt = now;

            await AddMovementAsync(item.WarehouseId, item, WarehouseStockMovementType.TransferOut, request.Quantity,
                currentUserId, occurredAt, now, fromBinId, toBin.Id, "Transfer", null, request.Note, null, cancellationToken);
            await AddMovementAsync(item.WarehouseId, dest, WarehouseStockMovementType.TransferIn, request.Quantity,
                currentUserId, occurredAt, now, fromBinId, toBin.Id, "Transfer", null, request.Note, null, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildItemDtoAsync(item.Id, cancellationToken);
    }

    public async Task<WarehouseStockItemDto> AdjustAsync(
        Guid currentUserId, bool isAdmin, Guid id, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var item = await LoadOwnedItemAsync(currentUserId, isAdmin, id, cancellationToken);

        if (request.NewQuantityOnHand.HasValue == request.Delta.HasValue)
        {
            throw new ConflictException("Provide exactly one of NewQuantityOnHand or Delta.");
        }

        var delta = request.Delta ?? request.NewQuantityOnHand!.Value - item.QuantityOnHand;
        if (delta == 0)
        {
            throw new ConflictException("The adjustment does not change the quantity.");
        }

        var newOnHand = item.QuantityOnHand + delta;
        if (newOnHand < 0)
        {
            throw new ConflictException("An adjustment cannot take on-hand quantity below zero.");
        }

        if (newOnHand < item.QuantityReserved)
        {
            throw new ConflictException(
                $"On-hand cannot drop below the reserved quantity ({item.QuantityReserved}).");
        }

        var movementType = string.IsNullOrWhiteSpace(request.MovementType)
            ? WarehouseStockMovementType.Adjustment
            : ParseEnum<WarehouseStockMovementType>(request.MovementType, "Invalid MovementType.");
        if (movementType is not (WarehouseStockMovementType.Adjustment or WarehouseStockMovementType.StockCount
            or WarehouseStockMovementType.Damage or WarehouseStockMovementType.Disposal))
        {
            throw new ConflictException("MovementType must be one of: Adjustment, StockCount, Damage, Disposal.");
        }

        var now = DateTime.UtcNow;
        var occurredAt = ToUtc(request.OccurredAt) ?? now;

        item.QuantityOnHand = newOnHand;
        RecomputeAvailable(item);
        item.LastMovementAt = occurredAt;
        item.UpdatedAt = now;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            item.Status = ParseEnum<WarehouseStockItemStatus>(request.Status, "Invalid Status.");
        }

        ApplyBinDelta(item.Bin, delta, now);
        ApplyWarehouseDelta(item.Warehouse, delta, now);

        await AddMovementAsync(item.WarehouseId, item, movementType, Math.Abs(delta),
            currentUserId, occurredAt, now, fromBinId: null, toBinId: null,
            referenceType: null, referenceId: null, request.Reason, request.Note, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildItemDtoAsync(item.Id, cancellationToken);
    }

    public Task<WarehouseStockItemDto> ReserveAsync(
        Guid currentUserId, bool isAdmin, Guid id, ReserveStockRequest request, CancellationToken cancellationToken)
        => ChangeReservationAsync(currentUserId, isAdmin, id, request, reserve: true, cancellationToken);

    public Task<WarehouseStockItemDto> ReleaseReservationAsync(
        Guid currentUserId, bool isAdmin, Guid id, ReserveStockRequest request, CancellationToken cancellationToken)
        => ChangeReservationAsync(currentUserId, isAdmin, id, request, reserve: false, cancellationToken);

    public async Task DeleteStockItemAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var item = await LoadOwnedItemAsync(currentUserId, isAdmin, id, cancellationToken);

        if (item.QuantityOnHand != 0 || item.QuantityReserved != 0)
        {
            throw new ConflictException("Issue or adjust the stock item to zero before deleting it.");
        }

        _repository.RemoveStockItem(item);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<WarehouseStockMovementDto>> GetMovementsAsync(
        Guid currentUserId, bool isAdmin, WarehouseStockMovementQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var profileId = await ResolveProfileIdAsync(currentUserId, isAdmin, cancellationToken);
        var (items, totalCount) = await _repository.GetMovementsPagedAsync(profileId, query, cancellationToken);

        return new PagedResult<WarehouseStockMovementDto>
        {
            Items = items.Select(m => m.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    // ---- shared plumbing ----------------------------------------

    private async Task<WarehouseStockItemDto> ChangeReservationAsync(
        Guid currentUserId, bool isAdmin, Guid id, ReserveStockRequest request, bool reserve, CancellationToken cancellationToken)
    {
        var item = await LoadOwnedItemAsync(currentUserId, isAdmin, id, cancellationToken);

        if (request.Quantity <= 0)
        {
            throw new ConflictException("Quantity must be greater than zero.");
        }

        if (reserve && request.Quantity > item.QuantityAvailable)
        {
            throw new ConflictException($"Only {item.QuantityAvailable} unit(s) are available to reserve.");
        }

        if (!reserve && request.Quantity > item.QuantityReserved)
        {
            throw new ConflictException($"Only {item.QuantityReserved} unit(s) are currently reserved.");
        }

        var now = DateTime.UtcNow;
        var occurredAt = ToUtc(request.OccurredAt) ?? now;

        item.QuantityReserved += reserve ? request.Quantity : -request.Quantity;
        RecomputeAvailable(item);
        item.LastMovementAt = occurredAt;
        item.UpdatedAt = now;

        await AddMovementAsync(item.WarehouseId, item,
            reserve ? WarehouseStockMovementType.Reserve : WarehouseStockMovementType.ReleaseReservation,
            request.Quantity, currentUserId, occurredAt, now, fromBinId: null, toBinId: null,
            request.ReferenceType, request.ReferenceId, reason: null, request.Note, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildItemDtoAsync(item.Id, cancellationToken);
    }

    private async Task<WarehouseStockItem> CreateStockItemAsync(
        Guid warehouseId, ReceiveStockRequest request, string sku, string? batch, DateTime now, CancellationToken cancellationToken)
    {
        var item = new WarehouseStockItem
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            WarehouseBinId = request.WarehouseBinId,
            ProductId = request.ProductId,
            OwnerUserId = request.OwnerUserId,
            Sku = sku,
            Description = request.Description.Trim(),
            UnitOfMeasure = string.IsNullOrWhiteSpace(request.UnitOfMeasure) ? "unit" : request.UnitOfMeasure.Trim(),
            BatchNumber = batch,
            ExpiryDate = ToUtc(request.ExpiryDate),
            Status = WarehouseStockItemStatus.Available,
            UnitValue = request.UnitValue,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddStockItemAsync(item, cancellationToken);
        return item;
    }

    private async Task<Guid?> ResolveProfileIdAsync(Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (isAdmin)
        {
            return null;
        }

        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        return profile.Id;
    }

    private async Task<WarehouseStockItem> LoadOwnedItemAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetStockItemByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Stock item not found.");

        if (!isAdmin)
        {
            var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new NotFoundException("Logistics partner profile not found.");

            if (item.Warehouse.LogisticsPartnerProfileId != profile.Id)
            {
                throw new UnauthorizedAccessException("This stock item belongs to another logistics partner.");
            }
        }

        return item;
    }

    private static void EnsureWarehouseOwned(Warehouse warehouse, Guid? profileId)
    {
        if (profileId.HasValue && warehouse.LogisticsPartnerProfileId != profileId.Value)
        {
            throw new UnauthorizedAccessException("This warehouse belongs to another logistics partner.");
        }
    }

    private static void RecomputeAvailable(WarehouseStockItem item)
        => item.QuantityAvailable = item.QuantityOnHand - item.QuantityReserved;

    private static void ApplyBinDelta(WarehouseBin? bin, int delta, DateTime now)
    {
        if (bin is null)
        {
            return;
        }

        bin.OccupiedUnits = Math.Max(0, bin.OccupiedUnits + delta);
        bin.UpdatedAt = now;
    }

    private static void ApplyWarehouseDelta(Warehouse? warehouse, int delta, DateTime now)
    {
        if (warehouse is null)
        {
            return;
        }

        warehouse.UsedCapacityUnits = Math.Max(0, warehouse.UsedCapacityUnits + delta);
        warehouse.UpdatedAt = now;
    }

    private async Task AddMovementAsync(
        Guid warehouseId, WarehouseStockItem item, WarehouseStockMovementType type, int quantity,
        Guid actorUserId, DateTime occurredAt, DateTime now, Guid? fromBinId, Guid? toBinId,
        string? referenceType, Guid? referenceId, string? reason, string? note, CancellationToken cancellationToken)
        => await _repository.AddMovementAsync(new WarehouseStockMovement
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            WarehouseStockItemId = item.Id,
            Type = type,
            Quantity = quantity,
            QuantityOnHandAfter = item.QuantityOnHand,
            FromBinId = fromBinId,
            ToBinId = toBinId,
            Sku = item.Sku,
            ReferenceType = string.IsNullOrWhiteSpace(referenceType) ? null : referenceType.Trim(),
            ReferenceId = referenceId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            PerformedByUserId = actorUserId,
            OccurredAt = occurredAt,
            CreatedAt = now,
        }, cancellationToken);

    private async Task<WarehouseStockItemDto> BuildItemDtoAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _repository.GetStockItemByIdAsync(itemId, cancellationToken);
        if (item is null)
        {
            // item was deleted as part of the operation
            return new WarehouseStockItemDto { Id = itemId };
        }

        var (movements, _) = await _repository.GetMovementsPagedAsync(
            profileId: null,
            new WarehouseStockMovementQueryParameters
            {
                WarehouseStockItemId = itemId,
                Page = 1,
                PageSize = RecentMovementsOnItem,
            },
            cancellationToken);

        return item.ToDto(movements);
    }

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}

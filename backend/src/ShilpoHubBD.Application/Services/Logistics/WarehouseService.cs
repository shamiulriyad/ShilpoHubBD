using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

/// <summary>
/// Warehouse Management for Logistics Partners: facilities, their zones and storage bins. Stock
/// holdings and the movement ledger live in <see cref="WarehouseStockService"/>. A partner manages
/// only their own warehouses; SuperAdmin manages any.
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;
    private readonly ILogisticsPartnerRepository _partnerRepository;

    public WarehouseService(
        IWarehouseRepository repository, ILogisticsPartnerRepository partnerRepository)
    {
        _repository = repository;
        _partnerRepository = partnerRepository;
    }

    public async Task<WarehouseDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new ConflictException("You must have a logistics partner profile to create warehouses.");

        var type = string.IsNullOrWhiteSpace(request.Type)
            ? WarehouseType.Distribution
            : ParseEnum<WarehouseType>(request.Type, "Invalid Type.");

        if (request.DistrictId.HasValue
            && !await _repository.DistrictExistsAsync(request.DistrictId.Value, cancellationToken))
        {
            throw new ConflictException("District not found.");
        }

        var now = DateTime.UtcNow;
        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = await UniqueCodeAsync(now, cancellationToken),
            LogisticsPartnerProfileId = profile.Id,
            CreatedByUserId = currentUserId,
            Name = request.Name.Trim(),
            Type = type,
            Status = WarehouseStatus.Active,
            AddressLine = request.AddressLine.Trim(),
            City = request.City.Trim(),
            DistrictId = request.DistrictId,
            PostalCode = request.PostalCode?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ContactPersonName = request.ContactPersonName?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            TotalCapacityUnits = request.TotalCapacityUnits < 0 ? 0 : request.TotalCapacityUnits,
            HasColdChain = request.HasColdChain,
            HandlesHazardous = request.HandlesHazardous,
            HandlesReturns = request.HandlesReturns,
            Notes = request.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(warehouse, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(warehouse.Id, cancellationToken);
    }

    public async Task<PagedResult<WarehouseListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, WarehouseQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        Guid? profileId = null;
        if (!isAdmin)
        {
            var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new NotFoundException("Logistics partner profile not found.");
            profileId = profile.Id;
        }

        var (items, totalCount) = await _repository.GetPagedAsync(profileId, query, cancellationToken);
        var counts = await _repository.GetCountsAsync(items.Select(w => w.Id), cancellationToken);

        return new PagedResult<WarehouseListItemDto>
        {
            Items = items.Select(w =>
            {
                var c = counts.GetValueOrDefault(w.Id);
                return w.ToListItemDto(c.Zones, c.Bins);
            }).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<WarehouseDto> GetByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        return await BuildDtoAsync(id, cancellationToken);
    }

    public async Task<WarehouseDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var warehouse = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            warehouse.Type = ParseEnum<WarehouseType>(request.Type, "Invalid Type.");
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            warehouse.Status = ParseEnum<WarehouseStatus>(request.Status, "Invalid Status.");
        }

        if (request.DistrictId.HasValue)
        {
            if (!await _repository.DistrictExistsAsync(request.DistrictId.Value, cancellationToken))
            {
                throw new ConflictException("District not found.");
            }

            warehouse.DistrictId = request.DistrictId;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            warehouse.Name = request.Name.Trim();
        }

        warehouse.AddressLine = Coalesce(request.AddressLine, warehouse.AddressLine);
        warehouse.City = Coalesce(request.City, warehouse.City);
        warehouse.PostalCode = request.PostalCode?.Trim() ?? warehouse.PostalCode;
        warehouse.ContactPersonName = request.ContactPersonName?.Trim() ?? warehouse.ContactPersonName;
        warehouse.ContactPhone = request.ContactPhone?.Trim() ?? warehouse.ContactPhone;

        if (request.Latitude.HasValue)
        {
            warehouse.Latitude = request.Latitude;
        }

        if (request.Longitude.HasValue)
        {
            warehouse.Longitude = request.Longitude;
        }

        if (request.TotalCapacityUnits.HasValue)
        {
            warehouse.TotalCapacityUnits = request.TotalCapacityUnits.Value < 0 ? 0 : request.TotalCapacityUnits.Value;
        }

        if (request.HasColdChain.HasValue)
        {
            warehouse.HasColdChain = request.HasColdChain.Value;
        }

        if (request.HandlesHazardous.HasValue)
        {
            warehouse.HandlesHazardous = request.HandlesHazardous.Value;
        }

        if (request.HandlesReturns.HasValue)
        {
            warehouse.HandlesReturns = request.HandlesReturns.Value;
        }

        warehouse.Notes = request.Notes?.Trim() ?? warehouse.Notes;
        warehouse.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildDtoAsync(warehouse.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (await _repository.HasStockItemsAsync(warehouse.Id, cancellationToken))
        {
            throw new ConflictException("Cannot delete a warehouse that still holds stock items.");
        }

        _repository.Remove(warehouse);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- Zones ---------------------------------------------------

    public async Task<WarehouseDto> AddZoneAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpsertWarehouseZoneRequest request, CancellationToken cancellationToken)
    {
        var warehouse = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var code = request.Code.Trim();

        if (warehouse.Zones.Any(z => string.Equals(z.Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A zone with code '{code}' already exists in this warehouse.");
        }

        var now = DateTime.UtcNow;
        warehouse.Zones.Add(new WarehouseZone
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouse.Id,
            Code = code,
            Name = request.Name.Trim(),
            Type = ParseEnum<WarehouseZoneType>(request.Type, "Invalid zone Type."),
            IsColdChain = request.IsColdChain,
            CapacityUnits = request.CapacityUnits < 0 ? 0 : request.CapacityUnits,
            IsActive = request.IsActive,
            Notes = request.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        });

        warehouse.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildDtoAsync(warehouse.Id, cancellationToken);
    }

    public async Task<WarehouseDto> UpdateZoneAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid zoneId, UpsertWarehouseZoneRequest request, CancellationToken cancellationToken)
    {
        var warehouse = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var zone = warehouse.Zones.FirstOrDefault(z => z.Id == zoneId)
            ?? throw new NotFoundException("Zone not found.");

        var code = request.Code.Trim();
        if (warehouse.Zones.Any(z => z.Id != zoneId && string.Equals(z.Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A zone with code '{code}' already exists in this warehouse.");
        }

        zone.Code = code;
        zone.Name = request.Name.Trim();
        zone.Type = ParseEnum<WarehouseZoneType>(request.Type, "Invalid zone Type.");
        zone.IsColdChain = request.IsColdChain;
        zone.CapacityUnits = request.CapacityUnits < 0 ? 0 : request.CapacityUnits;
        zone.IsActive = request.IsActive;
        zone.Notes = request.Notes?.Trim();
        zone.UpdatedAt = DateTime.UtcNow;
        warehouse.UpdatedAt = zone.UpdatedAt;

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildDtoAsync(warehouse.Id, cancellationToken);
    }

    public async Task<WarehouseDto> RemoveZoneAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid zoneId, CancellationToken cancellationToken)
    {
        var warehouse = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var zone = warehouse.Zones.FirstOrDefault(z => z.Id == zoneId)
            ?? throw new NotFoundException("Zone not found.");

        foreach (var bin in warehouse.Bins.Where(b => b.WarehouseZoneId == zoneId))
        {
            bin.WarehouseZoneId = null;
            bin.Zone = null;
        }

        warehouse.Zones.Remove(zone);
        warehouse.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildDtoAsync(warehouse.Id, cancellationToken);
    }

    // ---- Bins ---------------------------------------------------

    public async Task<WarehouseDto> AddBinAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpsertWarehouseBinRequest request, CancellationToken cancellationToken)
    {
        var warehouse = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var code = request.Code.Trim();

        if (warehouse.Bins.Any(b => string.Equals(b.Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A bin with code '{code}' already exists in this warehouse.");
        }

        if (request.WarehouseZoneId.HasValue
            && warehouse.Zones.All(z => z.Id != request.WarehouseZoneId.Value))
        {
            throw new ConflictException("Zone not found in this warehouse.");
        }

        var now = DateTime.UtcNow;
        warehouse.Bins.Add(new WarehouseBin
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouse.Id,
            WarehouseZoneId = request.WarehouseZoneId,
            Code = code,
            Label = request.Label?.Trim(),
            Type = ParseEnum<WarehouseBinType>(request.Type, "Invalid bin Type."),
            CapacityUnits = request.CapacityUnits < 0 ? 0 : request.CapacityUnits,
            IsPickable = request.IsPickable,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        });

        warehouse.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildDtoAsync(warehouse.Id, cancellationToken);
    }

    public async Task<WarehouseDto> UpdateBinAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid binId, UpsertWarehouseBinRequest request, CancellationToken cancellationToken)
    {
        var warehouse = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var bin = warehouse.Bins.FirstOrDefault(b => b.Id == binId)
            ?? throw new NotFoundException("Bin not found.");

        var code = request.Code.Trim();
        if (warehouse.Bins.Any(b => b.Id != binId && string.Equals(b.Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A bin with code '{code}' already exists in this warehouse.");
        }

        if (request.WarehouseZoneId.HasValue
            && warehouse.Zones.All(z => z.Id != request.WarehouseZoneId.Value))
        {
            throw new ConflictException("Zone not found in this warehouse.");
        }

        bin.WarehouseZoneId = request.WarehouseZoneId;
        bin.Code = code;
        bin.Label = request.Label?.Trim();
        bin.Type = ParseEnum<WarehouseBinType>(request.Type, "Invalid bin Type.");
        bin.CapacityUnits = request.CapacityUnits < 0 ? 0 : request.CapacityUnits;
        bin.IsPickable = request.IsPickable;
        bin.IsActive = request.IsActive;
        bin.UpdatedAt = DateTime.UtcNow;
        warehouse.UpdatedAt = bin.UpdatedAt;

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildDtoAsync(warehouse.Id, cancellationToken);
    }

    public async Task<WarehouseDto> RemoveBinAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid binId, CancellationToken cancellationToken)
    {
        var warehouse = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var bin = warehouse.Bins.FirstOrDefault(b => b.Id == binId)
            ?? throw new NotFoundException("Bin not found.");

        if (bin.OccupiedUnits > 0)
        {
            throw new ConflictException("Cannot delete a bin that still holds stock. Move or issue the stock first.");
        }

        warehouse.Bins.Remove(bin);
        warehouse.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return await BuildDtoAsync(warehouse.Id, cancellationToken);
    }

    // ---- helpers ----------------------------------------------

    private async Task<Warehouse> LoadOwnedAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Warehouse not found.");

        if (!isAdmin)
        {
            var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new NotFoundException("Logistics partner profile not found.");

            if (warehouse.LogisticsPartnerProfileId != profile.Id)
            {
                throw new UnauthorizedAccessException("This warehouse belongs to another logistics partner.");
            }
        }

        return warehouse;
    }

    private async Task<WarehouseDto> BuildDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = (await _repository.GetByIdAsync(id, cancellationToken))!;
        var counts = await _repository.GetCountsAsync(new[] { id }, cancellationToken);
        var stockItemCount = counts.GetValueOrDefault(id).StockItems;
        return warehouse.ToDto(stockItemCount);
    }

    private async Task<string> UniqueCodeAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"WH-{now:yyyyMM}-{Random.Shared.Next(0, 100000):D5}";
            if (!await _repository.CodeExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"WH-{now:yyyyMM}-{Guid.NewGuid():N}"[..20];
    }

    private static string Coalesce(string? value, string current)
        => string.IsNullOrWhiteSpace(value) ? current : value.Trim();

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}

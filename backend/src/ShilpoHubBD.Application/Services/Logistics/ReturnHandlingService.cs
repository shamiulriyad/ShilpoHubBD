using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

/// <summary>
/// Return Handling for Logistics Partners: raise a return, approve / reject, schedule the reverse
/// pickup, receive and inspect the goods, set per-item disposition, restock and record the refund. A
/// partner only ever sees their own returns; SuperAdmin sees all. Restock does not (yet) write into
/// warehouse stock — that link is a later integration item.
/// </summary>
public class ReturnHandlingService : IReturnHandlingService
{
    private static readonly Dictionary<ReturnStatus, ReturnStatus[]> Transitions = new()
    {
        [ReturnStatus.Requested] = new[] { ReturnStatus.Approved, ReturnStatus.Rejected, ReturnStatus.Cancelled },
        [ReturnStatus.Approved] = new[]
        {
            ReturnStatus.PickupScheduled, ReturnStatus.InTransit, ReturnStatus.Received, ReturnStatus.Cancelled,
        },
        [ReturnStatus.Rejected] = new[] { ReturnStatus.Closed },
        [ReturnStatus.PickupScheduled] = new[]
        {
            ReturnStatus.InTransit, ReturnStatus.Received, ReturnStatus.Cancelled,
        },
        [ReturnStatus.InTransit] = new[] { ReturnStatus.Received, ReturnStatus.Cancelled },
        [ReturnStatus.Received] = new[] { ReturnStatus.UnderInspection, ReturnStatus.Inspected, ReturnStatus.Cancelled },
        [ReturnStatus.UnderInspection] = new[] { ReturnStatus.Inspected, ReturnStatus.Cancelled },
        [ReturnStatus.Inspected] = new[]
        {
            ReturnStatus.Restocked, ReturnStatus.RefundPending, ReturnStatus.Refunded, ReturnStatus.Closed,
        },
        [ReturnStatus.Restocked] = new[] { ReturnStatus.RefundPending, ReturnStatus.Refunded, ReturnStatus.Closed },
        [ReturnStatus.RefundPending] = new[] { ReturnStatus.Refunded, ReturnStatus.Closed },
        [ReturnStatus.Refunded] = new[] { ReturnStatus.Closed },
        [ReturnStatus.Closed] = Array.Empty<ReturnStatus>(),
        [ReturnStatus.Cancelled] = Array.Empty<ReturnStatus>(),
    };

    private readonly IReturnHandlingRepository _repository;
    private readonly ILogisticsPartnerRepository _partnerRepository;

    public ReturnHandlingService(
        IReturnHandlingRepository repository, ILogisticsPartnerRepository partnerRepository)
    {
        _repository = repository;
        _partnerRepository = partnerRepository;
    }

    public async Task<ReturnRequestDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreateReturnRequestRequest request, CancellationToken cancellationToken)
    {
        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new ConflictException("You must have a logistics partner profile to raise returns.");

        if (request.Items.Count == 0)
        {
            throw new ConflictException("A return must have at least one item.");
        }

        var reason = ParseEnum<ReturnReason>(request.Reason, "Invalid Reason.");

        if (request.ShipmentId.HasValue
            && !await _repository.ShipmentBelongsToProfileAsync(request.ShipmentId.Value, profile.Id, cancellationToken))
        {
            throw new ConflictException("Shipment not found for this logistics partner.");
        }

        if (request.OrderId.HasValue && !await _repository.OrderExistsAsync(request.OrderId.Value, cancellationToken))
        {
            throw new ConflictException("Order not found.");
        }

        if (request.DestinationWarehouseId.HasValue
            && !await _repository.WarehouseBelongsToProfileAsync(request.DestinationWarehouseId.Value, profile.Id, cancellationToken))
        {
            throw new ConflictException("Destination warehouse not found for this logistics partner.");
        }

        await EnsureDistrictAsync(request.PickupDistrictId, cancellationToken);

        var now = DateTime.UtcNow;
        var returnRequest = new ReturnRequest
        {
            Id = Guid.NewGuid(),
            ReferenceCode = await UniqueReferenceAsync(now, cancellationToken),
            LogisticsPartnerProfileId = profile.Id,
            CreatedByUserId = currentUserId,
            ShipmentId = request.ShipmentId,
            OrderId = request.OrderId,
            DestinationWarehouseId = request.DestinationWarehouseId,
            Status = ReturnStatus.Requested,
            Reason = reason,
            ReasonDetail = request.ReasonDetail?.Trim(),
            CustomerName = request.CustomerName.Trim(),
            CustomerPhone = request.CustomerPhone.Trim(),
            PickupContactName = request.PickupContactName?.Trim(),
            PickupPhone = request.PickupPhone?.Trim(),
            PickupAddressLine = request.PickupAddressLine?.Trim(),
            PickupCity = request.PickupCity?.Trim(),
            PickupDistrictId = request.PickupDistrictId,
            PickupPostalCode = request.PickupPostalCode?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var itemInput in request.Items)
        {
            returnRequest.Items.Add(await BuildItemAsync(returnRequest.Id, itemInput, cancellationToken));
        }

        AddEvent(returnRequest, ReturnEventType.Created, currentUserId, now, null, ReturnStatus.Requested,
            "Return requested.");

        await _repository.AddAsync(returnRequest, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(returnRequest.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<ReturnRequestListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, ReturnRequestQueryParameters query, CancellationToken cancellationToken)
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

        return new PagedResult<ReturnRequestListItemDto>
        {
            Items = items.Select(r => r.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ReturnRequestDto> GetByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
        => (await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken)).ToDto();

    public async Task<ReturnRequestDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateReturnRequestRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (ret.Status != ReturnStatus.Requested)
        {
            throw new ConflictException("A return can only be edited while it is Requested.");
        }

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            ret.Reason = ParseEnum<ReturnReason>(request.Reason, "Invalid Reason.");
        }

        if (request.ShipmentId.HasValue
            && !await _repository.ShipmentBelongsToProfileAsync(request.ShipmentId.Value, ret.LogisticsPartnerProfileId, cancellationToken))
        {
            throw new ConflictException("Shipment not found for this logistics partner.");
        }

        if (request.OrderId.HasValue && !await _repository.OrderExistsAsync(request.OrderId.Value, cancellationToken))
        {
            throw new ConflictException("Order not found.");
        }

        if (request.DestinationWarehouseId.HasValue
            && !await _repository.WarehouseBelongsToProfileAsync(request.DestinationWarehouseId.Value, ret.LogisticsPartnerProfileId, cancellationToken))
        {
            throw new ConflictException("Destination warehouse not found for this logistics partner.");
        }

        if (request.PickupDistrictId.HasValue)
        {
            await EnsureDistrictAsync(request.PickupDistrictId, cancellationToken);
            ret.PickupDistrictId = request.PickupDistrictId;
        }

        if (request.ShipmentId.HasValue)
        {
            ret.ShipmentId = request.ShipmentId;
        }

        if (request.OrderId.HasValue)
        {
            ret.OrderId = request.OrderId;
        }

        if (request.DestinationWarehouseId.HasValue)
        {
            ret.DestinationWarehouseId = request.DestinationWarehouseId;
        }

        ret.ReasonDetail = request.ReasonDetail?.Trim() ?? ret.ReasonDetail;
        ret.CustomerName = Coalesce(request.CustomerName, ret.CustomerName);
        ret.CustomerPhone = Coalesce(request.CustomerPhone, ret.CustomerPhone);
        ret.PickupContactName = request.PickupContactName?.Trim() ?? ret.PickupContactName;
        ret.PickupPhone = request.PickupPhone?.Trim() ?? ret.PickupPhone;
        ret.PickupAddressLine = request.PickupAddressLine?.Trim() ?? ret.PickupAddressLine;
        ret.PickupCity = request.PickupCity?.Trim() ?? ret.PickupCity;
        ret.PickupPostalCode = request.PickupPostalCode?.Trim() ?? ret.PickupPostalCode;

        var now = DateTime.UtcNow;
        if (request.Items is not null)
        {
            if (request.Items.Count == 0)
            {
                throw new ConflictException("A return must have at least one item.");
            }

            ret.Items.Clear();
            foreach (var itemInput in request.Items)
            {
                ret.Items.Add(await BuildItemAsync(ret.Id, itemInput, cancellationToken));
            }
        }

        ret.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> ApproveAsync(
        Guid currentUserId, bool isAdmin, Guid id, ApproveReturnRequestRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureTransition(ret.Status, ReturnStatus.Approved);

        if (request.DestinationWarehouseId.HasValue)
        {
            if (!await _repository.WarehouseBelongsToProfileAsync(request.DestinationWarehouseId.Value, ret.LogisticsPartnerProfileId, cancellationToken))
            {
                throw new ConflictException("Destination warehouse not found for this logistics partner.");
            }

            ret.DestinationWarehouseId = request.DestinationWarehouseId;
        }

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.Status = ReturnStatus.Approved;
        ret.ApprovedByUserId = currentUserId;
        ret.ApprovedAt = now;
        ret.UpdatedAt = now;
        AddEvent(ret, ReturnEventType.Approved, currentUserId, now, from, ReturnStatus.Approved, request.Note?.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> RejectAsync(
        Guid currentUserId, bool isAdmin, Guid id, RejectReturnRequestRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureTransition(ret.Status, ReturnStatus.Rejected);

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.Status = ReturnStatus.Rejected;
        ret.RejectionReason = request.Reason.Trim();
        ret.UpdatedAt = now;
        AddEvent(ret, ReturnEventType.Rejected, currentUserId, now, from, ReturnStatus.Rejected, request.Reason.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> SchedulePickupAsync(
        Guid currentUserId, bool isAdmin, Guid id, ScheduleReturnPickupRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (ret.Status is not (ReturnStatus.Approved or ReturnStatus.PickupScheduled))
        {
            throw new ConflictException("A reverse pickup can only be scheduled once the return is Approved.");
        }

        await EnsureDistrictAsync(request.PickupDistrictId, cancellationToken);

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.ScheduledPickupAt = DateTime.SpecifyKind(request.ScheduledPickupAt, DateTimeKind.Utc);
        ret.PickupContactName = request.PickupContactName?.Trim() ?? ret.PickupContactName;
        ret.PickupPhone = request.PickupPhone?.Trim() ?? ret.PickupPhone;
        ret.PickupAddressLine = request.PickupAddressLine?.Trim() ?? ret.PickupAddressLine;
        ret.PickupCity = request.PickupCity?.Trim() ?? ret.PickupCity;
        ret.PickupPostalCode = request.PickupPostalCode?.Trim() ?? ret.PickupPostalCode;
        if (request.PickupDistrictId.HasValue)
        {
            ret.PickupDistrictId = request.PickupDistrictId;
        }

        ret.AssignedCarrierLabel = request.AssignedCarrierLabel?.Trim() ?? ret.AssignedCarrierLabel;
        ret.AssignedDriverName = request.AssignedDriverName?.Trim() ?? ret.AssignedDriverName;
        ret.Status = ReturnStatus.PickupScheduled;
        ret.UpdatedAt = now;
        AddEvent(ret, ReturnEventType.PickupScheduled, currentUserId, now, from, ReturnStatus.PickupScheduled,
            request.Note?.Trim() ?? $"Reverse pickup scheduled for {ret.ScheduledPickupAt:u}.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> UpdateStatusAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateReturnStatusRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var target = ParseEnum<ReturnStatus>(request.Status, "Invalid Status.");

        if (target is not (ReturnStatus.InTransit or ReturnStatus.Received
            or ReturnStatus.UnderInspection or ReturnStatus.Closed))
        {
            throw new ConflictException(
                "This endpoint only moves a return to InTransit, Received, UnderInspection or Closed. "
                + "Use the dedicated approve / reject / inspection / restock / refund / cancel endpoints otherwise.");
        }

        EnsureTransition(ret.Status, target);

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.Status = target;

        if (target == ReturnStatus.Received)
        {
            ret.ReceivedAt = ToUtc(request.ReceivedAt) ?? now;
            if (ret.ActualPickupAt is null)
            {
                ret.ActualPickupAt = ret.ReceivedAt;
            }
        }

        ret.UpdatedAt = now;
        AddEvent(ret, target == ReturnStatus.Closed ? ReturnEventType.Closed : ReturnEventType.StatusChanged,
            currentUserId, now, from, target, request.Note?.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> RecordInspectionAsync(
        Guid currentUserId, bool isAdmin, Guid id, RecordReturnInspectionRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (ret.Status is not (ReturnStatus.Received or ReturnStatus.UnderInspection or ReturnStatus.Inspected))
        {
            throw new ConflictException("Goods must be Received before they can be inspected.");
        }

        var overall = ParseEnum<ReturnItemCondition>(request.OverallCondition, "Invalid OverallCondition.");
        var resolution = ParseEnum<ReturnResolutionType>(request.RecommendedResolution, "Invalid RecommendedResolution.");

        foreach (var assessment in request.ItemAssessments)
        {
            var item = ret.Items.FirstOrDefault(i => i.Id == assessment.ReturnItemId)
                ?? throw new ConflictException($"Return item {assessment.ReturnItemId} not found on this return.");

            if (assessment.QuantityReceived.HasValue)
            {
                item.QuantityReceived = Math.Clamp(assessment.QuantityReceived.Value, 0, item.Quantity);
            }

            if (!string.IsNullOrWhiteSpace(assessment.Condition))
            {
                item.Condition = ParseEnum<ReturnItemCondition>(assessment.Condition, "Invalid item Condition.");
            }

            if (!string.IsNullOrWhiteSpace(assessment.Disposition))
            {
                item.Disposition = ParseEnum<ReturnDisposition>(assessment.Disposition, "Invalid item Disposition.");
            }

            if (assessment.UnitRefundAmount.HasValue)
            {
                item.UnitRefundAmount = assessment.UnitRefundAmount;
            }

            item.Notes = assessment.Notes?.Trim() ?? item.Notes;
        }

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.Inspections.Add(new ReturnInspection
        {
            Id = Guid.NewGuid(),
            ReturnRequestId = ret.Id,
            InspectedByUserId = currentUserId,
            InspectedAt = ToUtc(request.InspectedAt) ?? now,
            OverallCondition = overall,
            Summary = request.Summary.Trim(),
            RecommendedResolution = resolution,
            PhotosJson = string.IsNullOrWhiteSpace(request.PhotosJson) ? null : request.PhotosJson.Trim(),
            CreatedAt = now,
        });

        ret.ResolutionType ??= resolution;
        ret.Status = ReturnStatus.Inspected;
        ret.UpdatedAt = now;
        AddEvent(ret, ReturnEventType.InspectionCompleted, currentUserId, now, from, ReturnStatus.Inspected,
            $"Inspection: {overall}, recommend {resolution}.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> RestockAsync(
        Guid currentUserId, bool isAdmin, Guid id, RestockReturnRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (ret.Status is not (ReturnStatus.Inspected or ReturnStatus.Restocked))
        {
            throw new ConflictException("A return must be Inspected before it can be restocked.");
        }

        if (request.DestinationWarehouseId.HasValue)
        {
            if (!await _repository.WarehouseBelongsToProfileAsync(request.DestinationWarehouseId.Value, ret.LogisticsPartnerProfileId, cancellationToken))
            {
                throw new ConflictException("Destination warehouse not found for this logistics partner.");
            }

            ret.DestinationWarehouseId = request.DestinationWarehouseId;
        }

        foreach (var line in request.Items)
        {
            var item = ret.Items.FirstOrDefault(i => i.Id == line.ReturnItemId)
                ?? throw new ConflictException($"Return item {line.ReturnItemId} not found on this return.");

            if (item.Disposition != ReturnDisposition.Restock)
            {
                throw new ConflictException(
                    $"Item '{item.Description}' is dispositioned '{item.Disposition}', not Restock.");
            }

            var cap = item.QuantityReceived > 0 ? item.QuantityReceived : item.Quantity;
            item.RestockedQuantity = Math.Clamp(line.RestockedQuantity, 0, cap);
        }

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.Status = ReturnStatus.Restocked;
        ret.UpdatedAt = now;
        var totalRestocked = ret.Items.Sum(i => i.RestockedQuantity);
        AddEvent(ret, ReturnEventType.Restocked, currentUserId, now, from, ReturnStatus.Restocked,
            request.Note?.Trim() ?? $"{totalRestocked} unit(s) marked restocked.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> RecordRefundAsync(
        Guid currentUserId, bool isAdmin, Guid id, RecordReturnRefundRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (ret.Status is not (ReturnStatus.Inspected or ReturnStatus.Restocked or ReturnStatus.RefundPending))
        {
            throw new ConflictException("A refund can only be recorded after inspection.");
        }

        if (request.RefundAmount < 0)
        {
            throw new ConflictException("RefundAmount cannot be negative.");
        }

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.RefundAmount = request.RefundAmount;
        ret.RefundMethod = request.RefundMethod?.Trim() ?? ret.RefundMethod;
        ret.RefundReference = request.RefundReference?.Trim() ?? ret.RefundReference;
        ret.ResolutionNote = request.ResolutionNote?.Trim() ?? ret.ResolutionNote;

        if (!string.IsNullOrWhiteSpace(request.ResolutionType))
        {
            ret.ResolutionType = ParseEnum<ReturnResolutionType>(request.ResolutionType, "Invalid ResolutionType.");
        }

        if (request.MarkPaid)
        {
            EnsureTransition(ret.Status, ReturnStatus.Refunded);
            ret.Status = ReturnStatus.Refunded;
            ret.RefundedAt = ToUtc(request.RefundedAt) ?? now;
            AddEvent(ret, ReturnEventType.RefundCompleted, currentUserId, now, from, ReturnStatus.Refunded,
                $"Refund of {request.RefundAmount:0.00} paid.");
        }
        else
        {
            EnsureTransition(ret.Status, ReturnStatus.RefundPending);
            ret.Status = ReturnStatus.RefundPending;
            AddEvent(ret, ReturnEventType.RefundInitiated, currentUserId, now, from, ReturnStatus.RefundPending,
                $"Refund of {request.RefundAmount:0.00} initiated.");
        }

        ret.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> CloseAsync(
        Guid currentUserId, bool isAdmin, Guid id, CloseReturnRequestRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureTransition(ret.Status, ReturnStatus.Closed);

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.Status = ReturnStatus.Closed;
        ret.UpdatedAt = now;
        AddEvent(ret, ReturnEventType.Closed, currentUserId, now, from, ReturnStatus.Closed, request.Note?.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> CancelAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancelReturnRequestRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureTransition(ret.Status, ReturnStatus.Cancelled);

        var now = DateTime.UtcNow;
        var from = ret.Status;
        ret.Status = ReturnStatus.Cancelled;
        ret.CancellationReason = request.Reason.Trim();
        ret.UpdatedAt = now;
        AddEvent(ret, ReturnEventType.Cancelled, currentUserId, now, from, ReturnStatus.Cancelled, request.Reason.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task<ReturnRequestDto> AddNoteAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddReturnNoteRequest request, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var now = DateTime.UtcNow;
        AddEvent(ret, ReturnEventType.NoteAdded, currentUserId, now, null, null, request.Note.Trim());
        ret.UpdatedAt = now;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(ret.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var ret = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (ret.Status is not (ReturnStatus.Requested or ReturnStatus.Rejected or ReturnStatus.Cancelled))
        {
            throw new ConflictException("Only Requested, Rejected or Cancelled returns can be deleted.");
        }

        _repository.Remove(ret);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ----------------------------------------------

    private async Task<ReturnRequest> LoadOwnedAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var ret = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Return request not found.");

        if (!isAdmin)
        {
            var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new NotFoundException("Logistics partner profile not found.");

            if (ret.LogisticsPartnerProfileId != profile.Id)
            {
                throw new UnauthorizedAccessException("This return belongs to another logistics partner.");
            }
        }

        return ret;
    }

    private async Task<ReturnItem> BuildItemAsync(
        Guid returnRequestId, ReturnItemInput input, CancellationToken cancellationToken)
    {
        if (input.ProductId.HasValue
            && !await _repository.ProductExistsAsync(input.ProductId.Value, cancellationToken))
        {
            throw new ConflictException("Product not found.");
        }

        return new ReturnItem
        {
            Id = Guid.NewGuid(),
            ReturnRequestId = returnRequestId,
            ProductId = input.ProductId,
            Sku = input.Sku?.Trim(),
            Description = input.Description.Trim(),
            Quantity = input.Quantity < 1 ? 1 : input.Quantity,
            Condition = ReturnItemCondition.NotReceived,
            Disposition = ReturnDisposition.Pending,
            UnitRefundAmount = input.UnitRefundAmount,
            Notes = input.Notes?.Trim(),
        };
    }

    private async Task EnsureDistrictAsync(Guid? districtId, CancellationToken cancellationToken)
    {
        if (districtId.HasValue && !await _repository.DistrictExistsAsync(districtId.Value, cancellationToken))
        {
            throw new ConflictException("Pickup district not found.");
        }
    }

    private static void EnsureTransition(ReturnStatus from, ReturnStatus to)
    {
        if (!Transitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
        {
            throw new ConflictException($"Cannot move a return from {from} to {to}.");
        }
    }

    private static void AddEvent(
        ReturnRequest ret, ReturnEventType type, Guid actorUserId, DateTime now,
        ReturnStatus? from, ReturnStatus? to, string? note)
        => ret.Events.Add(new ReturnEvent
        {
            Id = Guid.NewGuid(),
            ReturnRequestId = ret.Id,
            Type = type,
            FromStatus = from,
            ToStatus = to,
            Note = string.IsNullOrWhiteSpace(note) ? null : note,
            ActorUserId = actorUserId,
            CreatedAt = now,
        });

    private async Task<string> UniqueReferenceAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"RTN-{now:yyyyMM}-{Random.Shared.Next(0, 100000):D5}";
            if (!await _repository.ReferenceExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"RTN-{now:yyyyMM}-{Guid.NewGuid():N}"[..20];
    }

    private static string Coalesce(string? value, string current)
        => string.IsNullOrWhiteSpace(value) ? current : value.Trim();

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}

using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

/// <summary>
/// Pickup Scheduling for Logistics Partners: create collection requests, place them on the schedule,
/// assign a driver / vehicle and drive the request through its lifecycle to Collected / Completed.
/// A partner only ever sees and manages their own requests; SuperAdmin sees all.
/// </summary>
public class PickupSchedulingService : IPickupSchedulingService
{
    private static readonly Dictionary<PickupRequestStatus, PickupRequestStatus[]> Transitions = new()
    {
        [PickupRequestStatus.Draft] = new[] { PickupRequestStatus.Scheduled, PickupRequestStatus.Cancelled },
        [PickupRequestStatus.Scheduled] = new[]
        {
            PickupRequestStatus.Assigned, PickupRequestStatus.EnRoute,
            PickupRequestStatus.Cancelled, PickupRequestStatus.Failed,
        },
        [PickupRequestStatus.Assigned] = new[]
        {
            PickupRequestStatus.Scheduled, PickupRequestStatus.EnRoute,
            PickupRequestStatus.Cancelled, PickupRequestStatus.Failed,
        },
        [PickupRequestStatus.EnRoute] = new[]
        {
            PickupRequestStatus.Collected, PickupRequestStatus.Cancelled, PickupRequestStatus.Failed,
        },
        [PickupRequestStatus.Collected] = new[] { PickupRequestStatus.Completed, PickupRequestStatus.Failed },
        [PickupRequestStatus.Completed] = Array.Empty<PickupRequestStatus>(),
        [PickupRequestStatus.Cancelled] = Array.Empty<PickupRequestStatus>(),
        [PickupRequestStatus.Failed] = Array.Empty<PickupRequestStatus>(),
    };

    private readonly IPickupRequestRepository _repository;
    private readonly ILogisticsPartnerRepository _partnerRepository;

    public PickupSchedulingService(
        IPickupRequestRepository repository, ILogisticsPartnerRepository partnerRepository)
    {
        _repository = repository;
        _partnerRepository = partnerRepository;
    }

    public async Task<PickupRequestDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreatePickupRequestRequest request, CancellationToken cancellationToken)
    {
        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new ConflictException("You must have a logistics partner profile to create pickup requests.");

        var priority = string.IsNullOrWhiteSpace(request.Priority)
            ? PickupPriority.Standard
            : ParseEnum<PickupPriority>(request.Priority, "Invalid Priority.");

        if (request.OrderId.HasValue
            && !await _repository.OrderExistsAsync(request.OrderId.Value, cancellationToken))
        {
            throw new ConflictException("Order not found.");
        }

        if (request.OriginProducerUserId.HasValue
            && !await _repository.UserExistsAsync(request.OriginProducerUserId.Value, cancellationToken))
        {
            throw new ConflictException("Origin producer user not found.");
        }

        await EnsureDistrictAsync(request.OriginDistrictId, "Origin district not found.", cancellationToken);
        await EnsureDistrictAsync(request.DestinationDistrictId, "Destination district not found.", cancellationToken);

        if (request.IsCashOnDelivery && (request.CodAmount is null or <= 0))
        {
            throw new ConflictException("CodAmount is required and must be greater than zero for cash-on-delivery pickups.");
        }

        var now = DateTime.UtcNow;
        var scheduledAt = ToUtc(request.ScheduledPickupAt);
        var windowEnd = ToUtc(request.PickupWindowEnd);
        if (scheduledAt.HasValue && windowEnd.HasValue && windowEnd.Value <= scheduledAt.Value)
        {
            throw new ConflictException("PickupWindowEnd must be after ScheduledPickupAt.");
        }

        var pickup = new PickupRequest
        {
            Id = Guid.NewGuid(),
            ReferenceCode = await UniqueReferenceAsync(now, cancellationToken),
            LogisticsPartnerProfileId = profile.Id,
            RequestedByUserId = currentUserId,
            Status = scheduledAt.HasValue ? PickupRequestStatus.Scheduled : PickupRequestStatus.Draft,
            Priority = priority,
            OrderId = request.OrderId,
            OriginProducerUserId = request.OriginProducerUserId,
            OriginContactName = request.OriginContactName.Trim(),
            OriginPhone = request.OriginPhone.Trim(),
            OriginAddressLine = request.OriginAddressLine.Trim(),
            OriginCity = request.OriginCity.Trim(),
            OriginDistrictId = request.OriginDistrictId,
            OriginPostalCode = request.OriginPostalCode?.Trim(),
            DestinationContactName = request.DestinationContactName?.Trim(),
            DestinationPhone = request.DestinationPhone?.Trim(),
            DestinationAddressLine = request.DestinationAddressLine?.Trim(),
            DestinationCity = request.DestinationCity?.Trim(),
            DestinationDistrictId = request.DestinationDistrictId,
            ScheduledPickupAt = scheduledAt,
            PickupWindowEnd = windowEnd,
            PackageCount = request.PackageCount < 1 ? 1 : request.PackageCount,
            TotalWeightKg = request.TotalWeightKg,
            DeclaredValue = request.DeclaredValue,
            RequiresColdChain = request.RequiresColdChain,
            IsFragile = request.IsFragile,
            IsCashOnDelivery = request.IsCashOnDelivery,
            CodAmount = request.IsCashOnDelivery ? request.CodAmount : null,
            SpecialInstructions = request.SpecialInstructions?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var item in request.Items)
        {
            pickup.Items.Add(BuildItem(pickup.Id, item));
        }

        AddEvent(pickup, PickupEventType.Created, currentUserId, now, null, PickupRequestStatus.Draft, "Pickup request created.");
        if (scheduledAt.HasValue)
        {
            AddEvent(pickup, PickupEventType.Scheduled, currentUserId, now,
                PickupRequestStatus.Draft, PickupRequestStatus.Scheduled,
                $"Scheduled for {scheduledAt:u}.");
        }

        await _repository.AddAsync(pickup, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(pickup.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<PickupRequestListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, PickupRequestQueryParameters query, CancellationToken cancellationToken)
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

        return new PagedResult<PickupRequestListItemDto>
        {
            Items = items.Select(r => r.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PickupRequestDto> GetByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var pickup = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        return pickup.ToDto();
    }

    public async Task<PickupRequestDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdatePickupRequestRequest request, CancellationToken cancellationToken)
    {
        var pickup = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (pickup.Status is not (PickupRequestStatus.Draft or PickupRequestStatus.Scheduled or PickupRequestStatus.Assigned))
        {
            throw new ConflictException("Only Draft, Scheduled or Assigned pickup requests can be edited.");
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            pickup.Priority = ParseEnum<PickupPriority>(request.Priority, "Invalid Priority.");
        }

        if (request.OrderId.HasValue)
        {
            if (!await _repository.OrderExistsAsync(request.OrderId.Value, cancellationToken))
            {
                throw new ConflictException("Order not found.");
            }

            pickup.OrderId = request.OrderId;
        }

        if (request.OriginProducerUserId.HasValue)
        {
            if (!await _repository.UserExistsAsync(request.OriginProducerUserId.Value, cancellationToken))
            {
                throw new ConflictException("Origin producer user not found.");
            }

            pickup.OriginProducerUserId = request.OriginProducerUserId;
        }

        if (request.OriginDistrictId.HasValue)
        {
            await EnsureDistrictAsync(request.OriginDistrictId, "Origin district not found.", cancellationToken);
            pickup.OriginDistrictId = request.OriginDistrictId;
        }

        if (request.DestinationDistrictId.HasValue)
        {
            await EnsureDistrictAsync(request.DestinationDistrictId, "Destination district not found.", cancellationToken);
            pickup.DestinationDistrictId = request.DestinationDistrictId;
        }

        pickup.OriginContactName = Coalesce(request.OriginContactName, pickup.OriginContactName);
        pickup.OriginPhone = Coalesce(request.OriginPhone, pickup.OriginPhone);
        pickup.OriginAddressLine = Coalesce(request.OriginAddressLine, pickup.OriginAddressLine);
        pickup.OriginCity = Coalesce(request.OriginCity, pickup.OriginCity);
        pickup.OriginPostalCode = request.OriginPostalCode?.Trim() ?? pickup.OriginPostalCode;
        pickup.DestinationContactName = request.DestinationContactName?.Trim() ?? pickup.DestinationContactName;
        pickup.DestinationPhone = request.DestinationPhone?.Trim() ?? pickup.DestinationPhone;
        pickup.DestinationAddressLine = request.DestinationAddressLine?.Trim() ?? pickup.DestinationAddressLine;
        pickup.DestinationCity = request.DestinationCity?.Trim() ?? pickup.DestinationCity;

        if (request.PackageCount.HasValue)
        {
            pickup.PackageCount = request.PackageCount.Value < 1 ? 1 : request.PackageCount.Value;
        }

        if (request.TotalWeightKg.HasValue)
        {
            pickup.TotalWeightKg = request.TotalWeightKg;
        }

        if (request.DeclaredValue.HasValue)
        {
            pickup.DeclaredValue = request.DeclaredValue;
        }

        if (request.RequiresColdChain.HasValue)
        {
            pickup.RequiresColdChain = request.RequiresColdChain.Value;
        }

        if (request.IsFragile.HasValue)
        {
            pickup.IsFragile = request.IsFragile.Value;
        }

        if (request.IsCashOnDelivery.HasValue)
        {
            pickup.IsCashOnDelivery = request.IsCashOnDelivery.Value;
            if (!pickup.IsCashOnDelivery)
            {
                pickup.CodAmount = null;
            }
        }

        if (request.CodAmount.HasValue)
        {
            pickup.CodAmount = request.CodAmount;
        }

        if (pickup.IsCashOnDelivery && (pickup.CodAmount is null or <= 0))
        {
            throw new ConflictException("CodAmount is required and must be greater than zero for cash-on-delivery pickups.");
        }

        pickup.SpecialInstructions = request.SpecialInstructions?.Trim() ?? pickup.SpecialInstructions;

        var now = DateTime.UtcNow;
        if (request.Items is not null)
        {
            pickup.Items.Clear();
            foreach (var item in request.Items)
            {
                pickup.Items.Add(BuildItem(pickup.Id, item));
            }

            AddEvent(pickup, PickupEventType.ItemsUpdated, currentUserId, now, null, null,
                $"Item list replaced ({request.Items.Count} line(s)).");
        }

        pickup.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(pickup.Id, cancellationToken))!.ToDto();
    }

    public async Task<PickupRequestDto> ScheduleAsync(
        Guid currentUserId, bool isAdmin, Guid id, SchedulePickupRequestRequest request, CancellationToken cancellationToken)
    {
        var pickup = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (pickup.Status is not (PickupRequestStatus.Draft or PickupRequestStatus.Scheduled or PickupRequestStatus.Assigned))
        {
            throw new ConflictException("Only Draft, Scheduled or Assigned pickup requests can be (re)scheduled.");
        }

        var scheduledAt = DateTime.SpecifyKind(request.ScheduledPickupAt, DateTimeKind.Utc);
        var windowEnd = ToUtc(request.PickupWindowEnd);
        if (windowEnd.HasValue && windowEnd.Value <= scheduledAt)
        {
            throw new ConflictException("PickupWindowEnd must be after ScheduledPickupAt.");
        }

        var now = DateTime.UtcNow;
        var wasDraft = pickup.Status == PickupRequestStatus.Draft;
        var from = pickup.Status;

        pickup.ScheduledPickupAt = scheduledAt;
        pickup.PickupWindowEnd = windowEnd;
        if (wasDraft)
        {
            pickup.Status = PickupRequestStatus.Scheduled;
        }

        pickup.UpdatedAt = now;
        AddEvent(pickup,
            wasDraft ? PickupEventType.Scheduled : PickupEventType.Rescheduled,
            currentUserId, now, from, pickup.Status,
            request.Note?.Trim() ?? $"Pickup window set to {scheduledAt:u}.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(pickup.Id, cancellationToken))!.ToDto();
    }

    public async Task<PickupRequestDto> AssignAsync(
        Guid currentUserId, bool isAdmin, Guid id, AssignPickupRequestRequest request, CancellationToken cancellationToken)
    {
        var pickup = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (pickup.Status is not (PickupRequestStatus.Scheduled or PickupRequestStatus.Assigned or PickupRequestStatus.EnRoute))
        {
            throw new ConflictException("A crew can only be assigned once the request is Scheduled.");
        }

        var now = DateTime.UtcNow;
        var from = pickup.Status;

        pickup.AssignedDriverName = request.AssignedDriverName.Trim();
        pickup.AssignedDriverPhone = request.AssignedDriverPhone?.Trim();
        pickup.AssignedVehicleLabel = request.AssignedVehicleLabel?.Trim();
        pickup.AssignedAt = now;
        if (pickup.Status == PickupRequestStatus.Scheduled)
        {
            pickup.Status = PickupRequestStatus.Assigned;
        }

        pickup.UpdatedAt = now;
        AddEvent(pickup, PickupEventType.Assigned, currentUserId, now, from, pickup.Status,
            request.Note?.Trim() ?? $"Assigned to {pickup.AssignedDriverName}.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(pickup.Id, cancellationToken))!.ToDto();
    }

    public async Task<PickupRequestDto> UpdateStatusAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdatePickupStatusRequest request, CancellationToken cancellationToken)
    {
        var pickup = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        var target = ParseEnum<PickupRequestStatus>(
            request.Status, "Status must be one of: EnRoute, Collected, Completed, Failed.");

        if (target is not (PickupRequestStatus.EnRoute or PickupRequestStatus.Collected
            or PickupRequestStatus.Completed or PickupRequestStatus.Failed))
        {
            throw new ConflictException("This endpoint only moves a request to EnRoute, Collected, Completed or Failed.");
        }

        EnsureTransition(pickup.Status, target);

        if (target == PickupRequestStatus.Failed && string.IsNullOrWhiteSpace(request.FailureReason))
        {
            throw new ConflictException("FailureReason is required when marking a pickup as Failed.");
        }

        var now = DateTime.UtcNow;
        var from = pickup.Status;
        pickup.Status = target;

        if (target == PickupRequestStatus.Collected)
        {
            pickup.ActualPickupAt = ToUtc(request.ActualPickupAt) ?? now;
        }

        if (target == PickupRequestStatus.Failed)
        {
            pickup.FailureReason = request.FailureReason!.Trim();
        }

        pickup.UpdatedAt = now;
        AddEvent(pickup, PickupEventType.StatusChanged, currentUserId, now, from, target, request.Note?.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(pickup.Id, cancellationToken))!.ToDto();
    }

    public async Task<PickupRequestDto> CancelAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancelPickupRequestRequest request, CancellationToken cancellationToken)
    {
        var pickup = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        EnsureTransition(pickup.Status, PickupRequestStatus.Cancelled);

        var now = DateTime.UtcNow;
        var from = pickup.Status;
        pickup.Status = PickupRequestStatus.Cancelled;
        pickup.CancellationReason = request.Reason.Trim();
        pickup.UpdatedAt = now;
        AddEvent(pickup, PickupEventType.Cancelled, currentUserId, now, from, PickupRequestStatus.Cancelled,
            request.Reason.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(pickup.Id, cancellationToken))!.ToDto();
    }

    public async Task<PickupRequestDto> AddNoteAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddPickupNoteRequest request, CancellationToken cancellationToken)
    {
        var pickup = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        var now = DateTime.UtcNow;
        AddEvent(pickup, PickupEventType.NoteAdded, currentUserId, now, null, null, request.Note.Trim());
        pickup.UpdatedAt = now;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(pickup.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var pickup = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (pickup.Status is not (PickupRequestStatus.Draft or PickupRequestStatus.Cancelled))
        {
            throw new ConflictException("Only Draft or Cancelled pickup requests can be deleted.");
        }

        _repository.Remove(pickup);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers -----------------------------------------------------

    private async Task<PickupRequest> LoadOwnedAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var pickup = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Pickup request not found.");

        if (!isAdmin)
        {
            var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new NotFoundException("Logistics partner profile not found.");

            if (pickup.LogisticsPartnerProfileId != profile.Id)
            {
                throw new UnauthorizedAccessException("This pickup request belongs to another logistics partner.");
            }
        }

        return pickup;
    }

    private async Task EnsureDistrictAsync(Guid? districtId, string message, CancellationToken cancellationToken)
    {
        if (districtId.HasValue
            && await _repository.GetDistrictAsync(districtId.Value, cancellationToken) is null)
        {
            throw new ConflictException(message);
        }
    }

    private static void EnsureTransition(PickupRequestStatus from, PickupRequestStatus to)
    {
        if (!Transitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
        {
            throw new ConflictException($"Cannot move a pickup request from {from} to {to}.");
        }
    }

    private static PickupItem BuildItem(Guid pickupRequestId, PickupItemRequest item) => new()
    {
        Id = Guid.NewGuid(),
        PickupRequestId = pickupRequestId,
        Description = item.Description.Trim(),
        Quantity = item.Quantity < 1 ? 1 : item.Quantity,
        WeightKg = item.WeightKg,
        LengthCm = item.LengthCm,
        WidthCm = item.WidthCm,
        HeightCm = item.HeightCm,
        Reference = item.Reference?.Trim(),
        IsFragile = item.IsFragile,
    };

    private static void AddEvent(
        PickupRequest pickup, PickupEventType type, Guid actorUserId, DateTime now,
        PickupRequestStatus? from, PickupRequestStatus? to, string? note)
        => pickup.Events.Add(new PickupEvent
        {
            Id = Guid.NewGuid(),
            PickupRequestId = pickup.Id,
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
            var candidate = $"PU-{now:yyyyMM}-{Random.Shared.Next(0, 100000):D5}";
            if (!await _repository.ReferenceExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"PU-{now:yyyyMM}-{Guid.NewGuid():N}"[..20];
    }

    private static string Coalesce(string? value, string current)
        => string.IsNullOrWhiteSpace(value) ? current : value.Trim();

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}

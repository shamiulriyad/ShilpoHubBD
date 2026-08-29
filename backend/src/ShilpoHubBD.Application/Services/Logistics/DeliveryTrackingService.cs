using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

/// <summary>
/// Delivery Tracking for Logistics Partners: create shipments, drive them through the status
/// lifecycle, record scan / location checkpoints and delivery attempts, and expose a public,
/// PII-light tracking lookup by tracking number. A partner only ever sees their own shipments;
/// SuperAdmin sees all.
/// </summary>
public class DeliveryTrackingService : IDeliveryTrackingService
{
    private static readonly Dictionary<ShipmentStatus, ShipmentStatus[]> Transitions = new()
    {
        [ShipmentStatus.Created] = new[] { ShipmentStatus.LabelCreated, ShipmentStatus.PickedUp, ShipmentStatus.Cancelled },
        [ShipmentStatus.LabelCreated] = new[] { ShipmentStatus.PickedUp, ShipmentStatus.Cancelled },
        [ShipmentStatus.PickedUp] = new[]
        {
            ShipmentStatus.InTransit, ShipmentStatus.AtHub, ShipmentStatus.OutForDelivery, ShipmentStatus.Cancelled,
        },
        [ShipmentStatus.InTransit] = new[]
        {
            ShipmentStatus.AtHub, ShipmentStatus.OutForDelivery, ShipmentStatus.DeliveryFailed, ShipmentStatus.Cancelled,
        },
        [ShipmentStatus.AtHub] = new[]
        {
            ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery, ShipmentStatus.Cancelled,
        },
        [ShipmentStatus.OutForDelivery] = new[]
        {
            ShipmentStatus.Delivered, ShipmentStatus.DeliveryFailed, ShipmentStatus.AtHub, ShipmentStatus.InTransit,
        },
        [ShipmentStatus.DeliveryFailed] = new[]
        {
            ShipmentStatus.OutForDelivery, ShipmentStatus.AtHub, ShipmentStatus.InTransit,
            ShipmentStatus.Returned, ShipmentStatus.Delivered, ShipmentStatus.Cancelled,
        },
        [ShipmentStatus.Delivered] = Array.Empty<ShipmentStatus>(),
        [ShipmentStatus.Returned] = Array.Empty<ShipmentStatus>(),
        [ShipmentStatus.Cancelled] = Array.Empty<ShipmentStatus>(),
    };

    private readonly IDeliveryTrackingRepository _repository;
    private readonly ILogisticsPartnerRepository _partnerRepository;

    public DeliveryTrackingService(
        IDeliveryTrackingRepository repository, ILogisticsPartnerRepository partnerRepository)
    {
        _repository = repository;
        _partnerRepository = partnerRepository;
    }

    public async Task<ShipmentDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new ConflictException("You must have a logistics partner profile to create shipments.");

        var serviceLevel = string.IsNullOrWhiteSpace(request.ServiceLevel)
            ? ShipmentServiceLevel.Standard
            : ParseEnum<ShipmentServiceLevel>(request.ServiceLevel, "Invalid ServiceLevel.");

        if (request.OrderId.HasValue && !await _repository.OrderExistsAsync(request.OrderId.Value, cancellationToken))
        {
            throw new ConflictException("Order not found.");
        }

        if (request.PickupRequestId.HasValue
            && !await _repository.PickupRequestBelongsToProfileAsync(request.PickupRequestId.Value, profile.Id, cancellationToken))
        {
            throw new ConflictException("Pickup request not found for this logistics partner.");
        }

        if (request.DeliveryRouteId.HasValue
            && !await _repository.RouteBelongsToProfileAsync(request.DeliveryRouteId.Value, profile.Id, cancellationToken))
        {
            throw new ConflictException("Delivery route not found for this logistics partner.");
        }

        await EnsureDistrictAsync(request.OriginDistrictId, "Origin district not found.", cancellationToken);
        await EnsureDistrictAsync(request.DestinationDistrictId, "Destination district not found.", cancellationToken);

        if (request.IsCashOnDelivery && (request.CodAmount is null or <= 0))
        {
            throw new ConflictException("CodAmount is required and must be greater than zero for cash-on-delivery shipments.");
        }

        var now = DateTime.UtcNow;
        var status = request.LabelCreated ? ShipmentStatus.LabelCreated : ShipmentStatus.Created;

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = await UniqueTrackingNumberAsync(now, cancellationToken),
            LogisticsPartnerProfileId = profile.Id,
            CreatedByUserId = currentUserId,
            Status = status,
            ServiceLevel = serviceLevel,
            OrderId = request.OrderId,
            PickupRequestId = request.PickupRequestId,
            DeliveryRouteId = request.DeliveryRouteId,
            OriginContactName = request.OriginContactName.Trim(),
            OriginPhone = request.OriginPhone.Trim(),
            OriginAddressLine = request.OriginAddressLine.Trim(),
            OriginCity = request.OriginCity.Trim(),
            OriginDistrictId = request.OriginDistrictId,
            OriginPostalCode = request.OriginPostalCode?.Trim(),
            RecipientName = request.RecipientName.Trim(),
            RecipientPhone = request.RecipientPhone.Trim(),
            DestinationAddressLine = request.DestinationAddressLine.Trim(),
            DestinationCity = request.DestinationCity.Trim(),
            DestinationDistrictId = request.DestinationDistrictId,
            DestinationPostalCode = request.DestinationPostalCode?.Trim(),
            ParcelCount = request.ParcelCount < 1 ? 1 : request.ParcelCount,
            TotalWeightKg = request.TotalWeightKg,
            DimensionsNote = request.DimensionsNote?.Trim(),
            DeclaredValue = request.DeclaredValue,
            ShippingCost = request.ShippingCost,
            IsCashOnDelivery = request.IsCashOnDelivery,
            CodAmount = request.IsCashOnDelivery ? request.CodAmount : null,
            EstimatedDeliveryAt = ToUtc(request.EstimatedDeliveryAt),
            LastStatusAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        AddEvent(shipment, ShipmentEventType.Created, currentUserId, now, null, status,
            null, null, null, null, "Shipment created.");

        await _repository.AddAsync(shipment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<ShipmentListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, ShipmentQueryParameters query, CancellationToken cancellationToken)
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

        return new PagedResult<ShipmentListItemDto>
        {
            Items = items.Select(s => s.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ShipmentDto> GetByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
        => (await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken)).ToDto();

    public async Task<ShipmentTrackingDto> TrackByNumberAsync(string trackingNumber, CancellationToken cancellationToken)
    {
        var shipment = await _repository.GetByTrackingNumberAsync((trackingNumber ?? string.Empty).Trim(), cancellationToken)
            ?? throw new NotFoundException("No shipment found for that tracking number.");
        return shipment.ToTrackingDto();
    }

    public async Task<ShipmentDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateShipmentRequest request, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (shipment.Status is not (ShipmentStatus.Created or ShipmentStatus.LabelCreated))
        {
            throw new ConflictException("Shipment details can only be edited before pickup.");
        }

        if (!string.IsNullOrWhiteSpace(request.ServiceLevel))
        {
            shipment.ServiceLevel = ParseEnum<ShipmentServiceLevel>(request.ServiceLevel, "Invalid ServiceLevel.");
        }

        if (request.OrderId.HasValue)
        {
            if (!await _repository.OrderExistsAsync(request.OrderId.Value, cancellationToken))
            {
                throw new ConflictException("Order not found.");
            }

            shipment.OrderId = request.OrderId;
        }

        if (request.PickupRequestId.HasValue)
        {
            if (!await _repository.PickupRequestBelongsToProfileAsync(
                    request.PickupRequestId.Value, shipment.LogisticsPartnerProfileId, cancellationToken))
            {
                throw new ConflictException("Pickup request not found for this logistics partner.");
            }

            shipment.PickupRequestId = request.PickupRequestId;
        }

        if (request.DeliveryRouteId.HasValue)
        {
            if (!await _repository.RouteBelongsToProfileAsync(
                    request.DeliveryRouteId.Value, shipment.LogisticsPartnerProfileId, cancellationToken))
            {
                throw new ConflictException("Delivery route not found for this logistics partner.");
            }

            shipment.DeliveryRouteId = request.DeliveryRouteId;
        }

        if (request.OriginDistrictId.HasValue)
        {
            await EnsureDistrictAsync(request.OriginDistrictId, "Origin district not found.", cancellationToken);
            shipment.OriginDistrictId = request.OriginDistrictId;
        }

        if (request.DestinationDistrictId.HasValue)
        {
            await EnsureDistrictAsync(request.DestinationDistrictId, "Destination district not found.", cancellationToken);
            shipment.DestinationDistrictId = request.DestinationDistrictId;
        }

        shipment.OriginContactName = Coalesce(request.OriginContactName, shipment.OriginContactName);
        shipment.OriginPhone = Coalesce(request.OriginPhone, shipment.OriginPhone);
        shipment.OriginAddressLine = Coalesce(request.OriginAddressLine, shipment.OriginAddressLine);
        shipment.OriginCity = Coalesce(request.OriginCity, shipment.OriginCity);
        shipment.OriginPostalCode = request.OriginPostalCode?.Trim() ?? shipment.OriginPostalCode;
        shipment.RecipientName = Coalesce(request.RecipientName, shipment.RecipientName);
        shipment.RecipientPhone = Coalesce(request.RecipientPhone, shipment.RecipientPhone);
        shipment.DestinationAddressLine = Coalesce(request.DestinationAddressLine, shipment.DestinationAddressLine);
        shipment.DestinationCity = Coalesce(request.DestinationCity, shipment.DestinationCity);
        shipment.DestinationPostalCode = request.DestinationPostalCode?.Trim() ?? shipment.DestinationPostalCode;

        if (request.ParcelCount.HasValue)
        {
            shipment.ParcelCount = request.ParcelCount.Value < 1 ? 1 : request.ParcelCount.Value;
        }

        if (request.TotalWeightKg.HasValue)
        {
            shipment.TotalWeightKg = request.TotalWeightKg;
        }

        shipment.DimensionsNote = request.DimensionsNote?.Trim() ?? shipment.DimensionsNote;

        if (request.DeclaredValue.HasValue)
        {
            shipment.DeclaredValue = request.DeclaredValue;
        }

        if (request.ShippingCost.HasValue)
        {
            shipment.ShippingCost = request.ShippingCost;
        }

        if (request.IsCashOnDelivery.HasValue)
        {
            shipment.IsCashOnDelivery = request.IsCashOnDelivery.Value;
            if (!shipment.IsCashOnDelivery)
            {
                shipment.CodAmount = null;
            }
        }

        if (request.CodAmount.HasValue)
        {
            shipment.CodAmount = request.CodAmount;
        }

        if (shipment.IsCashOnDelivery && (shipment.CodAmount is null or <= 0))
        {
            throw new ConflictException("CodAmount is required and must be greater than zero for cash-on-delivery shipments.");
        }

        if (request.EstimatedDeliveryAt.HasValue)
        {
            shipment.EstimatedDeliveryAt = ToUtc(request.EstimatedDeliveryAt);
        }

        shipment.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task<ShipmentDto> UpdateStatusAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateShipmentStatusRequest request, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        var target = ParseEnum<ShipmentStatus>(request.Status, "Invalid Status.");
        if (target is ShipmentStatus.Created)
        {
            throw new ConflictException("A shipment cannot be moved back to Created.");
        }

        if (target is ShipmentStatus.Delivered)
        {
            throw new ConflictException("Use the deliver / delivery-attempt endpoints to mark a shipment Delivered.");
        }

        if (target is ShipmentStatus.Cancelled)
        {
            throw new ConflictException("Use the cancel endpoint to cancel a shipment.");
        }

        EnsureTransition(shipment.Status, target);
        await EnsureDistrictAsync(request.DistrictId, "District not found.", cancellationToken);

        if (target == ShipmentStatus.DeliveryFailed && string.IsNullOrWhiteSpace(request.FailureReason))
        {
            throw new ConflictException("FailureReason is required when moving a shipment to DeliveryFailed.");
        }

        var now = DateTime.UtcNow;
        var occurredAt = ToUtc(request.OccurredAt) ?? now;
        var from = shipment.Status;
        shipment.Status = target;
        shipment.LastStatusAt = now;

        if (target == ShipmentStatus.PickedUp && shipment.DispatchedAt is null)
        {
            shipment.DispatchedAt = occurredAt;
        }

        if (target == ShipmentStatus.Returned)
        {
            shipment.FailureReason = request.FailureReason?.Trim() ?? shipment.FailureReason;
        }

        if (target == ShipmentStatus.DeliveryFailed)
        {
            shipment.FailureReason = request.FailureReason!.Trim();
        }

        ApplyLocation(shipment, request.LocationLabel, request.Latitude, request.Longitude, updateCurrent: true);

        AddEvent(shipment, EventTypeForStatus(target), currentUserId, occurredAt, from, target,
            request.LocationLabel, request.Latitude, request.Longitude, request.DistrictId,
            request.Description?.Trim());

        shipment.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task<ShipmentDto> AddTrackingEventAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddShipmentTrackingEventRequest request, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureNotClosed(shipment);

        var eventType = ParseEnum<ShipmentEventType>(request.EventType, "Invalid EventType.");
        if (eventType is ShipmentEventType.Created or ShipmentEventType.StatusChanged
            or ShipmentEventType.Delivered or ShipmentEventType.Cancelled or ShipmentEventType.DeliveryAttempted)
        {
            throw new ConflictException("That event type is managed by its dedicated endpoint.");
        }

        await EnsureDistrictAsync(request.DistrictId, "District not found.", cancellationToken);

        var now = DateTime.UtcNow;
        var occurredAt = ToUtc(request.OccurredAt) ?? now;

        ApplyLocation(shipment, request.LocationLabel, request.Latitude, request.Longitude,
            updateCurrent: request.UpdateCurrentLocation);

        AddEvent(shipment, eventType, currentUserId, occurredAt, null, null,
            request.LocationLabel, request.Latitude, request.Longitude, request.DistrictId,
            request.Description?.Trim());

        shipment.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task<ShipmentDto> UpdateLocationAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateShipmentLocationRequest request, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureNotClosed(shipment);
        await EnsureDistrictAsync(request.DistrictId, "District not found.", cancellationToken);

        var now = DateTime.UtcNow;
        var occurredAt = ToUtc(request.OccurredAt) ?? now;

        ApplyLocation(shipment, request.LocationLabel, request.Latitude, request.Longitude, updateCurrent: true);
        AddEvent(shipment, ShipmentEventType.LocationUpdated, currentUserId, occurredAt, null, null,
            request.LocationLabel, request.Latitude, request.Longitude, request.DistrictId,
            $"In transit near {request.LocationLabel.Trim()}.");

        shipment.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task<ShipmentDto> RecordDeliveryAttemptAsync(
        Guid currentUserId, bool isAdmin, Guid id, RecordDeliveryAttemptRequest request, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (shipment.Status is not (ShipmentStatus.OutForDelivery or ShipmentStatus.DeliveryFailed))
        {
            throw new ConflictException("Delivery attempts can only be recorded while the shipment is OutForDelivery.");
        }

        var outcome = ParseEnum<DeliveryAttemptOutcome>(request.Outcome, "Invalid Outcome.");
        var now = DateTime.UtcNow;
        var attemptedAt = ToUtc(request.AttemptedAt) ?? now;

        shipment.DeliveryAttemptCount += 1;
        var attempt = new DeliveryAttempt
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipment.Id,
            AttemptNumber = shipment.DeliveryAttemptCount,
            Outcome = outcome,
            AttemptedAt = attemptedAt,
            Note = request.Note?.Trim(),
            NextAttemptAt = ToUtc(request.NextAttemptAt),
            RecordedByUserId = currentUserId,
            CreatedAt = now,
        };
        shipment.Attempts.Add(attempt);

        var from = shipment.Status;
        if (outcome == DeliveryAttemptOutcome.Delivered)
        {
            shipment.Status = ShipmentStatus.Delivered;
            shipment.DeliveredAt = attemptedAt;
            shipment.ReceivedByName = request.ReceivedByName?.Trim() ?? shipment.ReceivedByName;
            shipment.ProofOfDeliveryNote = request.ProofOfDeliveryNote?.Trim() ?? shipment.ProofOfDeliveryNote;
            shipment.SignatureImageUrl = request.SignatureImageUrl?.Trim() ?? shipment.SignatureImageUrl;
            ApplyCod(shipment, request.CodCollected, now);
            AddEvent(shipment, ShipmentEventType.Delivered, currentUserId, attemptedAt, from, ShipmentStatus.Delivered,
                null, null, null, null, $"Delivered (attempt {attempt.AttemptNumber}).");
        }
        else
        {
            shipment.Status = ShipmentStatus.DeliveryFailed;
            shipment.FailureReason = request.Note?.Trim() ?? outcome.ToString();
            AddEvent(shipment, ShipmentEventType.DeliveryAttempted, currentUserId, attemptedAt, from, ShipmentStatus.DeliveryFailed,
                null, null, null, null, $"Attempt {attempt.AttemptNumber} failed: {outcome}.");
        }

        shipment.LastStatusAt = now;
        shipment.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task<ShipmentDto> MarkDeliveredAsync(
        Guid currentUserId, bool isAdmin, Guid id, MarkShipmentDeliveredRequest request, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (shipment.Status is not (ShipmentStatus.OutForDelivery or ShipmentStatus.DeliveryFailed))
        {
            throw new ConflictException("Only a shipment that is OutForDelivery (or a failed attempt) can be marked Delivered.");
        }

        var now = DateTime.UtcNow;
        var deliveredAt = ToUtc(request.DeliveredAt) ?? now;
        var from = shipment.Status;

        shipment.Status = ShipmentStatus.Delivered;
        shipment.DeliveredAt = deliveredAt;
        shipment.LastStatusAt = now;
        shipment.ReceivedByName = request.ReceivedByName?.Trim() ?? shipment.ReceivedByName;
        shipment.ProofOfDeliveryNote = request.ProofOfDeliveryNote?.Trim() ?? shipment.ProofOfDeliveryNote;
        shipment.SignatureImageUrl = request.SignatureImageUrl?.Trim() ?? shipment.SignatureImageUrl;
        shipment.DeliveryAttemptCount += 1;
        ApplyCod(shipment, request.CodCollected, now);

        AddEvent(shipment, ShipmentEventType.Delivered, currentUserId, deliveredAt, from, ShipmentStatus.Delivered,
            null, null, null, null,
            string.IsNullOrWhiteSpace(shipment.ReceivedByName)
                ? "Delivered."
                : $"Delivered, received by {shipment.ReceivedByName}.");

        shipment.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task<ShipmentDto> CancelAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancelShipmentRequest request, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureTransition(shipment.Status, ShipmentStatus.Cancelled);

        var now = DateTime.UtcNow;
        var from = shipment.Status;
        shipment.Status = ShipmentStatus.Cancelled;
        shipment.CancellationReason = request.Reason.Trim();
        shipment.LastStatusAt = now;
        shipment.UpdatedAt = now;

        AddEvent(shipment, ShipmentEventType.Cancelled, currentUserId, now, from, ShipmentStatus.Cancelled,
            null, null, null, null, request.Reason.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task<ShipmentDto> AddNoteAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddShipmentNoteRequest request, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var now = DateTime.UtcNow;

        AddEvent(shipment, ShipmentEventType.NoteAdded, currentUserId, now, null, null,
            null, null, null, null, request.Note.Trim());

        shipment.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(shipment.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var shipment = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (shipment.Status is not (ShipmentStatus.Created or ShipmentStatus.LabelCreated or ShipmentStatus.Cancelled))
        {
            throw new ConflictException("Only Created, LabelCreated or Cancelled shipments can be deleted.");
        }

        _repository.Remove(shipment);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ---------------------------------------------------

    private async Task<Shipment> LoadOwnedAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var shipment = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Shipment not found.");

        if (!isAdmin)
        {
            var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new NotFoundException("Logistics partner profile not found.");

            if (shipment.LogisticsPartnerProfileId != profile.Id)
            {
                throw new UnauthorizedAccessException("This shipment belongs to another logistics partner.");
            }
        }

        return shipment;
    }

    private async Task EnsureDistrictAsync(Guid? districtId, string message, CancellationToken cancellationToken)
    {
        if (districtId.HasValue && !await _repository.DistrictExistsAsync(districtId.Value, cancellationToken))
        {
            throw new ConflictException(message);
        }
    }

    private static void EnsureNotClosed(Shipment shipment)
    {
        if (shipment.Status is ShipmentStatus.Delivered or ShipmentStatus.Returned or ShipmentStatus.Cancelled)
        {
            throw new ConflictException($"Shipment is {shipment.Status}; no further tracking events can be added.");
        }
    }

    private static void EnsureTransition(ShipmentStatus from, ShipmentStatus to)
    {
        if (!Transitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
        {
            throw new ConflictException($"Cannot move a shipment from {from} to {to}.");
        }
    }

    private static void ApplyCod(Shipment shipment, bool codCollected, DateTime now)
    {
        if (shipment.IsCashOnDelivery && codCollected && !shipment.CodCollected)
        {
            shipment.CodCollected = true;
            shipment.CodCollectedAt = now;
        }
    }

    private static void ApplyLocation(
        Shipment shipment, string? label, double? lat, double? lon, bool updateCurrent)
    {
        if (!updateCurrent)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            shipment.CurrentLocationLabel = label.Trim();
        }

        if (lat.HasValue)
        {
            shipment.CurrentLatitude = lat;
        }

        if (lon.HasValue)
        {
            shipment.CurrentLongitude = lon;
        }
    }

    private static ShipmentEventType EventTypeForStatus(ShipmentStatus status) => status switch
    {
        ShipmentStatus.PickedUp => ShipmentEventType.PickedUp,
        ShipmentStatus.AtHub => ShipmentEventType.ArrivedAtHub,
        ShipmentStatus.OutForDelivery => ShipmentEventType.OutForDelivery,
        ShipmentStatus.Returned => ShipmentEventType.Returned,
        ShipmentStatus.DeliveryFailed => ShipmentEventType.Exception,
        _ => ShipmentEventType.StatusChanged,
    };

    private static void AddEvent(
        Shipment shipment, ShipmentEventType type, Guid actorUserId, DateTime occurredAt,
        ShipmentStatus? from, ShipmentStatus? to,
        string? locationLabel, double? lat, double? lon, Guid? districtId, string? description)
        => shipment.Events.Add(new ShipmentTrackingEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipment.Id,
            EventType = type,
            FromStatus = from,
            ToStatus = to,
            LocationLabel = string.IsNullOrWhiteSpace(locationLabel) ? null : locationLabel.Trim(),
            Latitude = lat,
            Longitude = lon,
            DistrictId = districtId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            OccurredAt = occurredAt,
            RecordedByUserId = actorUserId,
            CreatedAt = DateTime.UtcNow,
        });

    private async Task<string> UniqueTrackingNumberAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"SHP-{now:yyyyMM}-{Random.Shared.Next(0, 100000):D5}";
            if (!await _repository.TrackingNumberExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"SHP-{now:yyyyMM}-{Guid.NewGuid():N}"[..20];
    }

    private static string Coalesce(string? value, string current)
        => string.IsNullOrWhiteSpace(value) ? current : value.Trim();

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}

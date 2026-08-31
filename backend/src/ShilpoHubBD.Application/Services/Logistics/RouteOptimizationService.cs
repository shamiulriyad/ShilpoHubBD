using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

/// <summary>
/// Operational route management for Logistics Partners: build a route of pickup / delivery stops,
/// sequence them manually or with the built-in nearest-neighbour optimiser (haversine legs), assign a
/// crew and drive the route + its stops through execution. The AI route optimiser is a later part;
/// this service never calls a model.
/// </summary>
public class RouteOptimizationService : IRouteOptimizationService
{
    private const double DefaultSpeedKmh = 25.0;
    private const double EarthRadiusKm = 6371.0;

    private static readonly Dictionary<DeliveryRouteStatus, DeliveryRouteStatus[]> Transitions = new()
    {
        [DeliveryRouteStatus.Draft] = new[] { DeliveryRouteStatus.Planned, DeliveryRouteStatus.Cancelled },
        [DeliveryRouteStatus.Planned] = new[] { DeliveryRouteStatus.Draft, DeliveryRouteStatus.Dispatched, DeliveryRouteStatus.Cancelled },
        [DeliveryRouteStatus.Dispatched] = new[] { DeliveryRouteStatus.Planned, DeliveryRouteStatus.InProgress, DeliveryRouteStatus.Cancelled },
        [DeliveryRouteStatus.InProgress] = new[] { DeliveryRouteStatus.Completed, DeliveryRouteStatus.Cancelled },
        [DeliveryRouteStatus.Completed] = Array.Empty<DeliveryRouteStatus>(),
        [DeliveryRouteStatus.Cancelled] = Array.Empty<DeliveryRouteStatus>(),
    };

    private readonly IRouteOptimizationRepository _repository;
    private readonly ILogisticsPartnerRepository _partnerRepository;

    public RouteOptimizationService(
        IRouteOptimizationRepository repository, ILogisticsPartnerRepository partnerRepository)
    {
        _repository = repository;
        _partnerRepository = partnerRepository;
    }

    public async Task<DeliveryRouteDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreateDeliveryRouteRequest request, CancellationToken cancellationToken)
    {
        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new ConflictException("You must have a logistics partner profile to create routes.");

        if (request.OriginDistrictId.HasValue
            && !await _repository.DistrictExistsAsync(request.OriginDistrictId.Value, cancellationToken))
        {
            throw new ConflictException("Origin district not found.");
        }

        var now = DateTime.UtcNow;
        var route = new DeliveryRoute
        {
            Id = Guid.NewGuid(),
            RouteCode = await UniqueRouteCodeAsync(now, cancellationToken),
            LogisticsPartnerProfileId = profile.Id,
            CreatedByUserId = currentUserId,
            Name = request.Name.Trim(),
            Status = DeliveryRouteStatus.Draft,
            ScheduledDate = ToUtc(request.ScheduledDate),
            PlannedStartAt = ToUtc(request.PlannedStartAt),
            PlannedEndAt = ToUtc(request.PlannedEndAt),
            StartLocationLabel = request.StartLocationLabel?.Trim(),
            StartLatitude = request.StartLatitude,
            StartLongitude = request.StartLongitude,
            EndLocationLabel = request.EndLocationLabel?.Trim(),
            EndLatitude = request.EndLatitude,
            EndLongitude = request.EndLongitude,
            OriginDistrictId = request.OriginDistrictId,
            VehicleCapacityKg = request.VehicleCapacityKg,
            OptimizationStrategy = "manual",
            Notes = request.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        var sequence = 1;
        foreach (var stopInput in request.Stops)
        {
            var stop = await BuildStopAsync(route.Id, profile.Id, stopInput, now, cancellationToken);
            stop.Sequence = sequence++;
            route.Stops.Add(stop);
        }

        RecomputeRollups(route);
        AddEvent(route, DeliveryRouteEventType.Created, currentUserId, now, null, null, null, "Route created.");

        await _repository.AddAsync(route, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<DeliveryRouteListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, DeliveryRouteQueryParameters query, CancellationToken cancellationToken)
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

        return new PagedResult<DeliveryRouteListItemDto>
        {
            Items = items.Select(r => r.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<DeliveryRouteDto> GetByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
        => (await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken)).ToDto();

    public async Task<DeliveryRouteDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateDeliveryRouteRequest request, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureEditable(route);

        if (request.OriginDistrictId.HasValue
            && !await _repository.DistrictExistsAsync(request.OriginDistrictId.Value, cancellationToken))
        {
            throw new ConflictException("Origin district not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            route.Name = request.Name.Trim();
        }

        route.ScheduledDate = ToUtc(request.ScheduledDate) ?? route.ScheduledDate;
        route.PlannedStartAt = ToUtc(request.PlannedStartAt) ?? route.PlannedStartAt;
        route.PlannedEndAt = ToUtc(request.PlannedEndAt) ?? route.PlannedEndAt;
        route.StartLocationLabel = request.StartLocationLabel?.Trim() ?? route.StartLocationLabel;
        route.EndLocationLabel = request.EndLocationLabel?.Trim() ?? route.EndLocationLabel;

        if (request.StartLatitude.HasValue)
        {
            route.StartLatitude = request.StartLatitude;
        }

        if (request.StartLongitude.HasValue)
        {
            route.StartLongitude = request.StartLongitude;
        }

        if (request.EndLatitude.HasValue)
        {
            route.EndLatitude = request.EndLatitude;
        }

        if (request.EndLongitude.HasValue)
        {
            route.EndLongitude = request.EndLongitude;
        }

        if (request.OriginDistrictId.HasValue)
        {
            route.OriginDistrictId = request.OriginDistrictId;
        }

        if (request.VehicleCapacityKg.HasValue)
        {
            route.VehicleCapacityKg = request.VehicleCapacityKg;
        }

        if (request.EstimatedDurationMinutes.HasValue)
        {
            route.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        }

        route.Notes = request.Notes?.Trim() ?? route.Notes;

        Touch(route);
        AddEvent(route, DeliveryRouteEventType.Updated, currentUserId, route.UpdatedAt, null, null, null, "Route details updated.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public async Task<DeliveryRouteDto> AddStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, RouteStopInput request, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureEditable(route);

        var now = DateTime.UtcNow;
        var stop = await BuildStopAsync(route.Id, route.LogisticsPartnerProfileId, request, now, cancellationToken);
        stop.Sequence = route.Stops.Count == 0 ? 1 : route.Stops.Max(s => s.Sequence) + 1;
        route.Stops.Add(stop);

        RecomputeRollups(route);
        route.OptimizationStrategy = "manual";
        Touch(route);
        AddEvent(route, DeliveryRouteEventType.StopAdded, currentUserId, now, stop.Id, null, null,
            $"Added {stop.StopType} stop at {stop.AddressLine}.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public async Task<DeliveryRouteDto> UpdateStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, UpdateRouteStopRequest request, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureEditable(route);

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId)
            ?? throw new NotFoundException("Route stop not found.");

        if (!string.IsNullOrWhiteSpace(request.StopType))
        {
            stop.StopType = ParseEnum<DeliveryRouteStopType>(request.StopType, "Invalid StopType.");
        }

        if (request.PickupRequestId.HasValue)
        {
            if (!await _repository.PickupRequestBelongsToProfileAsync(
                    request.PickupRequestId.Value, route.LogisticsPartnerProfileId, cancellationToken))
            {
                throw new ConflictException("Pickup request not found for this logistics partner.");
            }

            stop.PickupRequestId = request.PickupRequestId;
        }

        if (request.OrderId.HasValue)
        {
            if (!await _repository.OrderExistsAsync(request.OrderId.Value, cancellationToken))
            {
                throw new ConflictException("Order not found.");
            }

            stop.OrderId = request.OrderId;
        }

        if (request.DistrictId.HasValue)
        {
            if (!await _repository.DistrictExistsAsync(request.DistrictId.Value, cancellationToken))
            {
                throw new ConflictException("District not found.");
            }

            stop.DistrictId = request.DistrictId;
        }

        stop.ContactName = request.ContactName?.Trim() ?? stop.ContactName;
        stop.ContactPhone = request.ContactPhone?.Trim() ?? stop.ContactPhone;
        stop.AddressLine = Coalesce(request.AddressLine, stop.AddressLine);
        stop.City = Coalesce(request.City, stop.City);
        stop.PostalCode = request.PostalCode?.Trim() ?? stop.PostalCode;

        if (request.Latitude.HasValue)
        {
            stop.Latitude = request.Latitude;
        }

        if (request.Longitude.HasValue)
        {
            stop.Longitude = request.Longitude;
        }

        if (request.LoadKg.HasValue)
        {
            stop.LoadKg = request.LoadKg;
        }

        if (request.PackageCount.HasValue)
        {
            stop.PackageCount = request.PackageCount.Value < 0 ? 0 : request.PackageCount.Value;
        }

        stop.PlannedArrivalAt = ToUtc(request.PlannedArrivalAt) ?? stop.PlannedArrivalAt;
        stop.PlannedDepartureAt = ToUtc(request.PlannedDepartureAt) ?? stop.PlannedDepartureAt;

        if (request.ServiceDurationMinutes.HasValue)
        {
            stop.ServiceDurationMinutes = request.ServiceDurationMinutes;
        }

        stop.Instructions = request.Instructions?.Trim() ?? stop.Instructions;
        stop.UpdatedAt = DateTime.UtcNow;

        RecomputeRollups(route);
        Touch(route);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public async Task<DeliveryRouteDto> RemoveStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureEditable(route);

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId)
            ?? throw new NotFoundException("Route stop not found.");

        route.Stops.Remove(stop);
        Resequence(route);
        RecomputeRollups(route);
        route.OptimizationStrategy = "manual";
        Touch(route);
        AddEvent(route, DeliveryRouteEventType.StopRemoved, currentUserId, route.UpdatedAt, stopId, null, null,
            "Stop removed.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public async Task<DeliveryRouteDto> ResequenceAsync(
        Guid currentUserId, bool isAdmin, Guid id, ResequenceRouteRequest request, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureEditable(route);

        var ids = request.StopIdsInOrder;
        var existing = route.Stops.Select(s => s.Id).ToHashSet();
        if (ids.Count != existing.Count || ids.Distinct().Count() != ids.Count || !ids.All(existing.Contains))
        {
            throw new ConflictException("StopIdsInOrder must list every stop on the route exactly once.");
        }

        for (var i = 0; i < ids.Count; i++)
        {
            route.Stops.First(s => s.Id == ids[i]).Sequence = i + 1;
        }

        route.OptimizationStrategy = "manual";
        Touch(route);
        AddEvent(route, DeliveryRouteEventType.Resequenced, currentUserId, route.UpdatedAt, null, null, null,
            "Stops reordered manually.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public async Task<DeliveryRouteDto> OptimizeAsync(
        Guid currentUserId, bool isAdmin, Guid id, OptimizeRouteRequest request, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureEditable(route);

        var strategy = string.IsNullOrWhiteSpace(request.Strategy) ? "nearest-neighbor" : request.Strategy.Trim();
        if (!strategy.Equals("nearest-neighbor", StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("Only the 'nearest-neighbor' strategy is supported.");
        }

        if (route.Stops.Count < 2)
        {
            throw new ConflictException("A route needs at least two stops to optimise.");
        }

        var speed = request.AverageSpeedKmh is > 0 ? request.AverageSpeedKmh!.Value : DefaultSpeedKmh;

        var ordered = NearestNeighbourOrder(route);
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Sequence = i + 1;
        }

        RecomputeLegs(route, ordered, speed);
        route.OptimizationStrategy = "nearest-neighbor";
        Touch(route);
        AddEvent(route, DeliveryRouteEventType.Optimized, currentUserId, route.UpdatedAt, null, null, null,
            $"Optimised with nearest-neighbour; est. {route.TotalDistanceKm:0.0} km / {route.EstimatedDurationMinutes} min.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public async Task<DeliveryRouteDto> AssignAsync(
        Guid currentUserId, bool isAdmin, Guid id, AssignRouteRequest request, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (route.Status is not (DeliveryRouteStatus.Draft or DeliveryRouteStatus.Planned or DeliveryRouteStatus.Dispatched))
        {
            throw new ConflictException("A crew can only be assigned before the route starts.");
        }

        var now = DateTime.UtcNow;
        var from = route.Status;

        route.AssignedDriverName = request.AssignedDriverName.Trim();
        route.AssignedDriverPhone = request.AssignedDriverPhone?.Trim();
        route.AssignedVehicleLabel = request.AssignedVehicleLabel?.Trim();
        if (request.VehicleCapacityKg.HasValue)
        {
            route.VehicleCapacityKg = request.VehicleCapacityKg;
        }

        route.AssignedAt = now;
        if (route.Status == DeliveryRouteStatus.Draft)
        {
            route.Status = DeliveryRouteStatus.Planned;
        }

        Touch(route, now);
        AddEvent(route, DeliveryRouteEventType.Assigned, currentUserId, now, null, from, route.Status,
            request.Note?.Trim() ?? $"Assigned to {route.AssignedDriverName}.");

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public Task<DeliveryRouteDto> DispatchAsync(
        Guid currentUserId, bool isAdmin, Guid id, RouteTransitionRequest request, CancellationToken cancellationToken)
        => TransitionAsync(currentUserId, isAdmin, id, DeliveryRouteStatus.Dispatched, DeliveryRouteEventType.Dispatched,
            request.Note, cancellationToken, route =>
            {
                if (route.Stops.Count == 0)
                {
                    throw new ConflictException("Add at least one stop before dispatching.");
                }

                if (string.IsNullOrWhiteSpace(route.AssignedDriverName))
                {
                    throw new ConflictException("Assign a crew before dispatching.");
                }
            });

    public Task<DeliveryRouteDto> StartAsync(
        Guid currentUserId, bool isAdmin, Guid id, RouteTransitionRequest request, CancellationToken cancellationToken)
        => TransitionAsync(currentUserId, isAdmin, id, DeliveryRouteStatus.InProgress, DeliveryRouteEventType.Started,
            request.Note, cancellationToken, route => route.ActualStartAt = DateTime.UtcNow);

    public Task<DeliveryRouteDto> CompleteAsync(
        Guid currentUserId, bool isAdmin, Guid id, RouteTransitionRequest request, CancellationToken cancellationToken)
        => TransitionAsync(currentUserId, isAdmin, id, DeliveryRouteStatus.Completed, DeliveryRouteEventType.Completed,
            request.Note, cancellationToken, route =>
            {
                if (route.Stops.Any(s => s.Status is DeliveryRouteStopStatus.Pending or DeliveryRouteStopStatus.EnRoute or DeliveryRouteStopStatus.Arrived))
                {
                    throw new ConflictException("Every stop must be Completed, Skipped or Failed before the route can be completed.");
                }

                route.ActualEndAt = DateTime.UtcNow;
            });

    public async Task<DeliveryRouteDto> CancelAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancelRouteRequest request, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureTransition(route.Status, DeliveryRouteStatus.Cancelled);

        var now = DateTime.UtcNow;
        var from = route.Status;
        route.Status = DeliveryRouteStatus.Cancelled;
        route.CancellationReason = request.Reason.Trim();
        Touch(route, now);
        AddEvent(route, DeliveryRouteEventType.Cancelled, currentUserId, now, null, from, DeliveryRouteStatus.Cancelled,
            request.Reason.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public Task<DeliveryRouteDto> ArriveStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, CancellationToken cancellationToken)
        => StopOpAsync(currentUserId, isAdmin, id, stopId, DeliveryRouteEventType.StopArrived, cancellationToken, (route, stop, now) =>
        {
            if (stop.Status is not (DeliveryRouteStopStatus.Pending or DeliveryRouteStopStatus.EnRoute))
            {
                throw new ConflictException($"Cannot mark a {stop.Status} stop as Arrived.");
            }

            stop.Status = DeliveryRouteStopStatus.Arrived;
            stop.ActualArrivalAt = now;
            return $"Arrived at stop {stop.Sequence}.";
        });

    public Task<DeliveryRouteDto> CompleteStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, CompleteRouteStopRequest request, CancellationToken cancellationToken)
        => StopOpAsync(currentUserId, isAdmin, id, stopId, DeliveryRouteEventType.StopCompleted, cancellationToken, (route, stop, now) =>
        {
            if (IsStopTerminal(stop.Status))
            {
                throw new ConflictException($"Stop is already {stop.Status}.");
            }

            stop.Status = DeliveryRouteStopStatus.Completed;
            stop.ActualArrivalAt = ToUtc(request.ActualArrivalAt) ?? stop.ActualArrivalAt ?? now;
            stop.ActualDepartureAt = ToUtc(request.ActualDepartureAt) ?? now;
            stop.CompletionNote = request.CompletionNote?.Trim();
            return $"Completed stop {stop.Sequence}.";
        });

    public Task<DeliveryRouteDto> SkipStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, FailRouteStopRequest request, CancellationToken cancellationToken)
        => StopOpAsync(currentUserId, isAdmin, id, stopId, DeliveryRouteEventType.StopSkipped, cancellationToken, (route, stop, now) =>
        {
            if (IsStopTerminal(stop.Status))
            {
                throw new ConflictException($"Stop is already {stop.Status}.");
            }

            stop.Status = DeliveryRouteStopStatus.Skipped;
            stop.FailureReason = request.FailureReason.Trim();
            return $"Skipped stop {stop.Sequence}.";
        });

    public Task<DeliveryRouteDto> FailStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, FailRouteStopRequest request, CancellationToken cancellationToken)
        => StopOpAsync(currentUserId, isAdmin, id, stopId, DeliveryRouteEventType.StopFailed, cancellationToken, (route, stop, now) =>
        {
            if (IsStopTerminal(stop.Status))
            {
                throw new ConflictException($"Stop is already {stop.Status}.");
            }

            stop.Status = DeliveryRouteStopStatus.Failed;
            stop.FailureReason = request.FailureReason.Trim();
            return $"Failed stop {stop.Sequence}.";
        });

    public async Task<DeliveryRouteDto> AddNoteAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddRouteNoteRequest request, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        var now = DateTime.UtcNow;
        AddEvent(route, DeliveryRouteEventType.NoteAdded, currentUserId, now, null, null, null, request.Note.Trim());
        Touch(route, now);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (route.Status is not (DeliveryRouteStatus.Draft or DeliveryRouteStatus.Cancelled))
        {
            throw new ConflictException("Only Draft or Cancelled routes can be deleted.");
        }

        _repository.Remove(route);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- shared transition plumbing -------------------------------

    private async Task<DeliveryRouteDto> TransitionAsync(
        Guid currentUserId, bool isAdmin, Guid id, DeliveryRouteStatus target, DeliveryRouteEventType eventType,
        string? note, CancellationToken cancellationToken, Action<DeliveryRoute> apply)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);
        EnsureTransition(route.Status, target);

        var now = DateTime.UtcNow;
        var from = route.Status;
        apply(route);
        route.Status = target;
        Touch(route, now);
        AddEvent(route, eventType, currentUserId, now, null, from, target, note?.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    private async Task<DeliveryRouteDto> StopOpAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, DeliveryRouteEventType eventType,
        CancellationToken cancellationToken, Func<DeliveryRoute, DeliveryRouteStop, DateTime, string> apply)
    {
        var route = await LoadOwnedAsync(currentUserId, isAdmin, id, cancellationToken);

        if (route.Status != DeliveryRouteStatus.InProgress)
        {
            throw new ConflictException("Stops can only be actioned while the route is InProgress.");
        }

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId)
            ?? throw new NotFoundException("Route stop not found.");

        var now = DateTime.UtcNow;
        var note = apply(route, stop, now);
        stop.UpdatedAt = now;

        RecomputeRollups(route);
        Touch(route, now);
        AddEvent(route, eventType, currentUserId, now, stop.Id, null, null, note);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(route.Id, cancellationToken))!.ToDto();
    }

    // ---- helpers -------------------------------------------------

    private async Task<DeliveryRoute> LoadOwnedAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var route = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Route not found.");

        if (!isAdmin)
        {
            var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new NotFoundException("Logistics partner profile not found.");

            if (route.LogisticsPartnerProfileId != profile.Id)
            {
                throw new UnauthorizedAccessException("This route belongs to another logistics partner.");
            }
        }

        return route;
    }

    private async Task<DeliveryRouteStop> BuildStopAsync(
        Guid routeId, Guid profileId, RouteStopInput input, DateTime now, CancellationToken cancellationToken)
    {
        var stopType = ParseEnum<DeliveryRouteStopType>(input.StopType, "Invalid StopType.");

        if (input.PickupRequestId.HasValue
            && !await _repository.PickupRequestBelongsToProfileAsync(input.PickupRequestId.Value, profileId, cancellationToken))
        {
            throw new ConflictException("Pickup request not found for this logistics partner.");
        }

        if (input.OrderId.HasValue && !await _repository.OrderExistsAsync(input.OrderId.Value, cancellationToken))
        {
            throw new ConflictException("Order not found.");
        }

        if (input.DistrictId.HasValue && !await _repository.DistrictExistsAsync(input.DistrictId.Value, cancellationToken))
        {
            throw new ConflictException("District not found.");
        }

        return new DeliveryRouteStop
        {
            Id = Guid.NewGuid(),
            DeliveryRouteId = routeId,
            StopType = stopType,
            Status = DeliveryRouteStopStatus.Pending,
            PickupRequestId = input.PickupRequestId,
            OrderId = input.OrderId,
            ContactName = input.ContactName?.Trim(),
            ContactPhone = input.ContactPhone?.Trim(),
            AddressLine = input.AddressLine.Trim(),
            City = input.City.Trim(),
            DistrictId = input.DistrictId,
            PostalCode = input.PostalCode?.Trim(),
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            LoadKg = input.LoadKg,
            PackageCount = input.PackageCount < 0 ? 0 : input.PackageCount,
            PlannedArrivalAt = ToUtc(input.PlannedArrivalAt),
            PlannedDepartureAt = ToUtc(input.PlannedDepartureAt),
            ServiceDurationMinutes = input.ServiceDurationMinutes,
            Instructions = input.Instructions?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    private static void EnsureEditable(DeliveryRoute route)
    {
        if (route.Status is not (DeliveryRouteStatus.Draft or DeliveryRouteStatus.Planned))
        {
            throw new ConflictException("Stops and route details can only be changed while the route is Draft or Planned.");
        }
    }

    private static void EnsureTransition(DeliveryRouteStatus from, DeliveryRouteStatus to)
    {
        if (!Transitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
        {
            throw new ConflictException($"Cannot move a route from {from} to {to}.");
        }
    }

    private static void Resequence(DeliveryRoute route)
    {
        var i = 1;
        foreach (var stop in route.Stops.OrderBy(s => s.Sequence))
        {
            stop.Sequence = i++;
        }
    }

    private static void RecomputeRollups(DeliveryRoute route)
    {
        route.TotalStops = route.Stops.Count;
        route.CompletedStops = route.Stops.Count(s => s.Status == DeliveryRouteStopStatus.Completed);
        route.TotalLoadKg = route.Stops.Where(s => s.LoadKg.HasValue).Sum(s => s.LoadKg!.Value);
    }

    private static List<DeliveryRouteStop> NearestNeighbourOrder(DeliveryRoute route)
    {
        var withCoords = route.Stops.Where(s => s.Latitude.HasValue && s.Longitude.HasValue).ToList();
        var withoutCoords = route.Stops
            .Where(s => !s.Latitude.HasValue || !s.Longitude.HasValue)
            .OrderBy(s => s.Sequence)
            .ToList();

        var result = new List<DeliveryRouteStop>();
        double? currentLat = route.StartLatitude;
        double? currentLon = route.StartLongitude;

        if (currentLat is null || currentLon is null)
        {
            var seed = withCoords.OrderBy(s => s.Sequence).FirstOrDefault();
            if (seed is not null)
            {
                result.Add(seed);
                withCoords.Remove(seed);
                currentLat = seed.Latitude;
                currentLon = seed.Longitude;
            }
        }

        while (withCoords.Count > 0)
        {
            var lat = currentLat!.Value;
            var lon = currentLon!.Value;
            var next = withCoords
                .OrderBy(s => Haversine(lat, lon, s.Latitude!.Value, s.Longitude!.Value))
                .ThenBy(s => s.Sequence)
                .First();
            result.Add(next);
            withCoords.Remove(next);
            currentLat = next.Latitude;
            currentLon = next.Longitude;
        }

        result.AddRange(withoutCoords);
        return result;
    }

    private static void RecomputeLegs(DeliveryRoute route, List<DeliveryRouteStop> ordered, double speedKmh)
    {
        double totalKm = 0;
        double totalMinutes = 0;
        double? prevLat = route.StartLatitude;
        double? prevLon = route.StartLongitude;

        foreach (var stop in ordered)
        {
            if (prevLat.HasValue && prevLon.HasValue && stop.Latitude.HasValue && stop.Longitude.HasValue)
            {
                var legKm = Haversine(prevLat.Value, prevLon.Value, stop.Latitude.Value, stop.Longitude.Value);
                stop.DistanceFromPreviousKm = Math.Round((decimal)legKm, 2);
                totalKm += legKm;
                totalMinutes += legKm / speedKmh * 60.0;
            }
            else
            {
                stop.DistanceFromPreviousKm = null;
            }

            totalMinutes += stop.ServiceDurationMinutes ?? 0;

            if (stop.Latitude.HasValue && stop.Longitude.HasValue)
            {
                prevLat = stop.Latitude;
                prevLon = stop.Longitude;
            }
        }

        route.TotalDistanceKm = Math.Round((decimal)totalKm, 2);
        route.EstimatedDurationMinutes = (int)Math.Round(totalMinutes);
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
            * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static bool IsStopTerminal(DeliveryRouteStopStatus status)
        => status is DeliveryRouteStopStatus.Completed or DeliveryRouteStopStatus.Skipped or DeliveryRouteStopStatus.Failed;

    private static void AddEvent(
        DeliveryRoute route, DeliveryRouteEventType type, Guid actorUserId, DateTime now,
        Guid? stopId, DeliveryRouteStatus? from, DeliveryRouteStatus? to, string? note)
        => route.Events.Add(new DeliveryRouteEvent
        {
            Id = Guid.NewGuid(),
            DeliveryRouteId = route.Id,
            Type = type,
            RouteStopId = stopId,
            FromStatus = from,
            ToStatus = to,
            Note = string.IsNullOrWhiteSpace(note) ? null : note,
            ActorUserId = actorUserId,
            CreatedAt = now,
        });

    private async Task<string> UniqueRouteCodeAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"RT-{now:yyyyMM}-{Random.Shared.Next(0, 100000):D5}";
            if (!await _repository.RouteCodeExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"RT-{now:yyyyMM}-{Guid.NewGuid():N}"[..20];
    }

    private static void Touch(DeliveryRoute route) => route.UpdatedAt = DateTime.UtcNow;

    private static void Touch(DeliveryRoute route, DateTime now) => route.UpdatedAt = now;

    private static string Coalesce(string? value, string current)
        => string.IsNullOrWhiteSpace(value) ? current : value.Trim();

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}

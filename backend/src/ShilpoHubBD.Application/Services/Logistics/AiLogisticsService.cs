using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

/// <summary>
/// AI Logistics for Logistics Partners — Delivery Prediction, Route Optimization, Demand Forecast and
/// Smart Warehouse Allocation. Each feature gathers signals from the partner's own logistics data,
/// runs a pluggable rule-based provider (no real model) and optionally persists the result. A partner
/// only ever sees / acts on their own artefacts; SuperAdmin sees all.
/// </summary>
public class AiLogisticsService : IAiLogisticsService
{
    private static readonly string[] RouteObjectives = { "proximity", "balanced", "capacity", "coldchain", "cost" };
    private static readonly string[] NetworkMetrics = { "shipments", "pickups", "returns", "weight_kg" };
    private static readonly string[] WarehouseMetrics = { "inbound", "outbound" };

    private readonly IAiLogisticsRepository _repository;
    private readonly ILogisticsPartnerRepository _partnerRepository;
    private readonly IDeliveryPredictionProvider _deliveryPredictionProvider;
    private readonly IAiRouteOptimizationProvider _routeOptimizationProvider;
    private readonly IDemandForecastProvider _demandForecastProvider;
    private readonly IWarehouseAllocationProvider _warehouseAllocationProvider;

    public AiLogisticsService(
        IAiLogisticsRepository repository,
        ILogisticsPartnerRepository partnerRepository,
        IDeliveryPredictionProvider deliveryPredictionProvider,
        IAiRouteOptimizationProvider routeOptimizationProvider,
        IDemandForecastProvider demandForecastProvider,
        IWarehouseAllocationProvider warehouseAllocationProvider)
    {
        _repository = repository;
        _partnerRepository = partnerRepository;
        _deliveryPredictionProvider = deliveryPredictionProvider;
        _routeOptimizationProvider = routeOptimizationProvider;
        _demandForecastProvider = demandForecastProvider;
        _warehouseAllocationProvider = warehouseAllocationProvider;
    }

    // ================= Delivery prediction =================

    public async Task<DeliveryPredictionDto> PredictDeliveryAsync(
        Guid currentUserId, bool isAdmin, PredictDeliveryRequest request, CancellationToken cancellationToken)
    {
        var callerProfileId = await ResolveProfileIdAsync(currentUserId, isAdmin, cancellationToken);

        var shipment = await _repository.GetShipmentAsync(request.ShipmentId, cancellationToken)
            ?? throw new ConflictException("Shipment not found.");

        var owningProfileId = ResolveOwner(shipment.LogisticsPartnerProfileId, callerProfileId, isAdmin,
            "This shipment belongs to another logistics partner.");

        var lookback = Math.Clamp(request.LookbackDays ?? 120, 14, 365);
        var since = DateTime.UtcNow.AddDays(-lookback);

        var lane = await _repository.GetLaneStatsAsync(
            owningProfileId, shipment.OriginDistrictId, shipment.DestinationDistrictId, since, cancellationToken);
        var partner = await _repository.GetPartnerDeliveryStatsAsync(owningProfileId, since, cancellationToken);

        var input = new LogisticsDeliveryPredictionInput
        {
            ServiceLevel = shipment.ServiceLevel.ToString(),
            CurrentStatus = shipment.Status.ToString(),
            NowUtc = DateTime.UtcNow,
            DispatchedAt = shipment.DispatchedAt,
            PromisedDeliveryAt = shipment.EstimatedDeliveryAt,
            DeliveryAttemptCount = shipment.DeliveryAttemptCount,
            IsCashOnDelivery = shipment.IsCashOnDelivery,
            SameDistrict = shipment.OriginDistrictId.HasValue
                && shipment.OriginDistrictId == shipment.DestinationDistrictId,
            HistoricalTransitDaysAvg = lane.AverageTransitDays,
            HistoricalOnTimeRate = lane.OnTimeRate,
            HistoricalFailureRate = lane.FailureRate,
            LaneSampleSize = lane.SampleSize,
            PartnerOnTimeRate = partner.OnTimeRate,
            PartnerAvgAttempts = partner.AverageAttempts,
        };

        var result = _deliveryPredictionProvider.Predict(input);

        var now = DateTime.UtcNow;
        var prediction = new DeliveryPrediction
        {
            Id = Guid.NewGuid(),
            LogisticsPartnerProfileId = owningProfileId,
            ShipmentId = shipment.Id,
            GeneratedByUserId = currentUserId,
            Method = result.Method,
            PredictedDeliveryAt = result.PredictedDeliveryAt,
            PredictedTransitDays = result.PredictedTransitDays,
            OnTimeProbability = result.OnTimeProbability,
            PredictedFailureProbability = result.PredictedFailureProbability,
            RiskLevel = result.RiskLevel,
            Confidence = result.Confidence,
            Summary = result.Summary,
            FactorsJson = result.FactorsJson,
            CreatedAt = now,
        };

        if (!request.Persist)
        {
            prediction.Shipment = shipment;
            return prediction.ToDto();
        }

        await _repository.AddDeliveryPredictionAsync(prediction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetDeliveryPredictionByIdAsync(prediction.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<DeliveryPredictionListItemDto>> GetDeliveryPredictionsAsync(
        Guid currentUserId, bool isAdmin, AiLogisticsQueryParameters query, CancellationToken cancellationToken)
    {
        NormalisePaging(query);
        var profileId = await ScopeProfileIdAsync(currentUserId, isAdmin, cancellationToken);
        var (items, total) = await _repository.GetDeliveryPredictionsPagedAsync(profileId, query, cancellationToken);
        return Page(items.Select(i => i.ToListItemDto()).ToList(), total, query);
    }

    public async Task<DeliveryPredictionDto> GetDeliveryPredictionByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetDeliveryPredictionByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Delivery prediction not found.");
        await EnsureOwnedAsync(entity.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteDeliveryPredictionAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetDeliveryPredictionByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Delivery prediction not found.");
        await EnsureOwnedAsync(entity.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);
        _repository.RemoveDeliveryPrediction(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ================= Route optimization =================

    public async Task<RouteOptimizationRunDto> OptimizeRouteAsync(
        Guid currentUserId, bool isAdmin, OptimizeRouteAiRequest request, CancellationToken cancellationToken)
    {
        var callerProfileId = await ResolveProfileIdAsync(currentUserId, isAdmin, cancellationToken);

        var route = await _repository.GetRouteWithStopsAsync(request.DeliveryRouteId, cancellationToken)
            ?? throw new ConflictException("Delivery route not found.");
        var owningProfileId = ResolveOwner(route.LogisticsPartnerProfileId, callerProfileId, isAdmin,
            "This route belongs to another logistics partner.");

        if (route.Stops.Count < 2)
        {
            throw new ConflictException("A route needs at least two stops to optimise.");
        }

        var objective = (request.Objective ?? "proximity").Trim().ToLowerInvariant();
        if (!RouteObjectives.Contains(objective))
        {
            throw new ConflictException($"Objective must be one of: {string.Join(", ", RouteObjectives)}.");
        }

        var input = new AiRouteOptimizationInput
        {
            Objective = objective,
            AverageSpeedKmh = request.AverageSpeedKmh is > 0 ? request.AverageSpeedKmh!.Value : 25.0,
            StartLatitude = route.StartLatitude,
            StartLongitude = route.StartLongitude,
            Stops = route.Stops
                .OrderBy(s => s.Sequence)
                .Select(s => new AiRouteStopInput(
                    s.Id, s.Sequence, s.Latitude, s.Longitude,
                    s.ServiceDurationMinutes ?? 0,
                    $"{s.StopType} - {s.AddressLine}, {s.City}"))
                .ToList(),
        };

        var result = _routeOptimizationProvider.Optimize(input);

        var now = DateTime.UtcNow;
        var run = new RouteOptimizationRun
        {
            Id = Guid.NewGuid(),
            LogisticsPartnerProfileId = owningProfileId,
            DeliveryRouteId = route.Id,
            GeneratedByUserId = currentUserId,
            Status = RouteOptimizationRunStatus.Proposed,
            Method = result.Method,
            Objective = objective,
            Summary = result.Summary,
            OriginalDistanceKm = route.TotalDistanceKm ?? Round(result.OriginalDistanceKm),
            ProposedDistanceKm = Round(result.ProposedDistanceKm),
            DistanceSavingKm = Round(result.OriginalDistanceKm - result.ProposedDistanceKm),
            ProposedDurationMinutes = result.ProposedDurationMinutes,
            Confidence = result.Confidence,
            CreatedAt = now,
        };

        foreach (var s in result.OrderedStops)
        {
            run.Stops.Add(new RouteOptimizationRunStop
            {
                Id = Guid.NewGuid(),
                RouteOptimizationRunId = run.Id,
                RouteStopId = s.StopId,
                OriginalSequence = s.OriginalSequence,
                ProposedSequence = s.ProposedSequence,
                DistanceFromPreviousKm = s.DistanceFromPreviousKm.HasValue
                    ? Math.Round((decimal)s.DistanceFromPreviousKm.Value, 2)
                    : null,
                Label = s.Label,
            });
        }

        if (!request.Persist)
        {
            run.DeliveryRoute = route;
            return run.ToDto();
        }

        await _repository.AddRouteOptimizationRunAsync(run, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetRouteOptimizationRunByIdAsync(run.Id, cancellationToken))!.ToDto();
    }

    public async Task<RouteOptimizationRunDto> ApplyRouteOptimizationAsync(
        Guid currentUserId, bool isAdmin, Guid runId, CancellationToken cancellationToken)
    {
        var run = await _repository.GetRouteOptimizationRunByIdAsync(runId, cancellationToken)
            ?? throw new NotFoundException("Route optimization run not found.");
        await EnsureOwnedAsync(run.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);

        if (run.Status != RouteOptimizationRunStatus.Proposed)
        {
            throw new ConflictException($"This run is already {run.Status}.");
        }

        var route = await _repository.GetRouteWithStopsAsync(run.DeliveryRouteId, cancellationToken)
            ?? throw new ConflictException("The route this run targets no longer exists.");

        if (route.Status is not (DeliveryRouteStatus.Draft or DeliveryRouteStatus.Planned))
        {
            throw new ConflictException("A route can only be re-sequenced while it is Draft or Planned.");
        }

        var runStopIds = run.Stops.Select(s => s.RouteStopId).OrderBy(x => x).ToList();
        var routeStopIds = route.Stops.Select(s => s.Id).OrderBy(x => x).ToList();
        if (!runStopIds.SequenceEqual(routeStopIds))
        {
            throw new ConflictException("The route's stops have changed since this run was generated; re-run the optimiser.");
        }

        var now = DateTime.UtcNow;
        foreach (var runStop in run.Stops)
        {
            var stop = route.Stops.First(s => s.Id == runStop.RouteStopId);
            stop.Sequence = runStop.ProposedSequence;
            stop.DistanceFromPreviousKm = runStop.DistanceFromPreviousKm;
            stop.UpdatedAt = now;
        }

        route.OptimizationStrategy = "ai-2opt";
        route.TotalDistanceKm = run.ProposedDistanceKm ?? route.TotalDistanceKm;
        route.EstimatedDurationMinutes = run.ProposedDurationMinutes ?? route.EstimatedDurationMinutes;
        route.UpdatedAt = now;
        route.Events.Add(new DeliveryRouteEvent
        {
            Id = Guid.NewGuid(),
            DeliveryRouteId = route.Id,
            Type = DeliveryRouteEventType.Optimized,
            Note = $"Applied AI optimisation run {run.Id} ({run.Method}); est. {run.ProposedDistanceKm:0.0} km.",
            ActorUserId = currentUserId,
            CreatedAt = now,
        });

        run.Status = RouteOptimizationRunStatus.Applied;
        run.AppliedAt = now;
        run.AppliedByUserId = currentUserId;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetRouteOptimizationRunByIdAsync(run.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<RouteOptimizationRunListItemDto>> GetRouteOptimizationRunsAsync(
        Guid currentUserId, bool isAdmin, AiLogisticsQueryParameters query, CancellationToken cancellationToken)
    {
        NormalisePaging(query);
        var profileId = await ScopeProfileIdAsync(currentUserId, isAdmin, cancellationToken);
        var (items, total) = await _repository.GetRouteOptimizationRunsPagedAsync(profileId, query, cancellationToken);
        return Page(items.Select(i => i.ToListItemDto()).ToList(), total, query);
    }

    public async Task<RouteOptimizationRunDto> GetRouteOptimizationRunByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetRouteOptimizationRunByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Route optimization run not found.");
        await EnsureOwnedAsync(entity.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteRouteOptimizationRunAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetRouteOptimizationRunByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Route optimization run not found.");
        await EnsureOwnedAsync(entity.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);
        _repository.RemoveRouteOptimizationRun(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ================= Demand forecast =================

    public async Task<DemandForecastDto> ForecastDemandAsync(
        Guid currentUserId, bool isAdmin, ForecastDemandRequest request, CancellationToken cancellationToken)
    {
        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new ConflictException("You must have a logistics partner profile to run forecasts.");

        var scope = ParseEnum<DemandForecastScope>(request.Scope ?? "Network", "Invalid Scope.");
        var horizon = Math.Clamp(request.HorizonDays, 1, 180);
        var lookback = Math.Clamp(request.LookbackDays ?? 90, 14, 365);
        var granularity = (request.Granularity ?? "day").Trim().ToLowerInvariant();
        if (granularity is not ("day" or "week"))
        {
            throw new ConflictException("Granularity must be 'day' or 'week'.");
        }

        var metric = (request.Metric ?? (scope == DemandForecastScope.Warehouse ? "inbound" : "shipments"))
            .Trim().ToLowerInvariant();
        var allowedMetrics = scope == DemandForecastScope.Warehouse ? WarehouseMetrics : NetworkMetrics;
        if (!allowedMetrics.Contains(metric))
        {
            throw new ConflictException($"For scope {scope}, metric must be one of: {string.Join(", ", allowedMetrics)}.");
        }

        var scopeLabel = "Whole network";
        if (scope == DemandForecastScope.District)
        {
            if (!request.ScopeId.HasValue)
            {
                throw new ConflictException("ScopeId (district) is required for District scope.");
            }

            scopeLabel = await _repository.GetDistrictNameAsync(request.ScopeId.Value, cancellationToken)
                ?? throw new ConflictException("District not found.");
        }
        else if (scope == DemandForecastScope.Warehouse)
        {
            if (!request.ScopeId.HasValue
                || !await _repository.WarehouseBelongsToProfileAsync(request.ScopeId.Value, profile.Id, cancellationToken))
            {
                throw new ConflictException("ScopeId must be one of your warehouses for Warehouse scope.");
            }

            scopeLabel = await _repository.GetWarehouseLabelAsync(request.ScopeId.Value, cancellationToken) ?? "Warehouse";
        }

        var now = DateTime.UtcNow;
        var fromUtc = now.Date.AddDays(-lookback);
        var toUtc = now.Date.AddDays(1);

        var raw = await _repository.GetDailyDemandSeriesAsync(
            profile.Id, scope, request.ScopeId, metric, fromUtc, toUtc, cancellationToken);
        var byDate = raw.ToDictionary(r => r.Date.Date, r => r.Value);

        var history = new List<LogisticsDemandObservation>();
        for (var d = fromUtc.Date; d <= now.Date; d = d.AddDays(1))
        {
            history.Add(new LogisticsDemandObservation(d, byDate.GetValueOrDefault(d, 0)));
        }

        var result = _demandForecastProvider.Forecast(new LogisticsDemandForecastInput
        {
            Metric = metric,
            HorizonDays = horizon,
            Granularity = granularity,
            AsOf = now,
            History = history,
        });

        var forecast = new DemandForecast
        {
            Id = Guid.NewGuid(),
            LogisticsPartnerProfileId = profile.Id,
            GeneratedByUserId = currentUserId,
            Scope = scope,
            ScopeId = request.ScopeId,
            ScopeLabel = scopeLabel,
            Metric = metric,
            HorizonDays = horizon,
            Granularity = granularity,
            Method = result.Method,
            Summary = result.Summary,
            Confidence = result.Confidence,
            BaselineDailyAverage = result.BaselineDailyAverage,
            PredictedTotal = result.PredictedTotal,
            AssumptionsJson = result.AssumptionsJson,
            PeriodStart = result.Points.Count > 0 ? result.Points[0].PeriodDate : now.Date.AddDays(1),
            PeriodEnd = result.Points.Count > 0 ? result.Points[^1].PeriodDate : now.Date.AddDays(horizon),
            CreatedAt = now,
        };

        foreach (var p in result.Points)
        {
            forecast.Points.Add(new DemandForecastPoint
            {
                Id = Guid.NewGuid(),
                DemandForecastId = forecast.Id,
                PeriodDate = p.PeriodDate,
                PredictedValue = p.PredictedValue,
                LowerBound = p.LowerBound,
                UpperBound = p.UpperBound,
            });
        }

        if (!request.Persist)
        {
            return forecast.ToDto();
        }

        await _repository.AddDemandForecastAsync(forecast, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetDemandForecastByIdAsync(forecast.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<DemandForecastListItemDto>> GetDemandForecastsAsync(
        Guid currentUserId, bool isAdmin, AiLogisticsQueryParameters query, CancellationToken cancellationToken)
    {
        NormalisePaging(query);
        var profileId = await ScopeProfileIdAsync(currentUserId, isAdmin, cancellationToken);
        var (items, total) = await _repository.GetDemandForecastsPagedAsync(profileId, query, cancellationToken);
        return Page(items.Select(i => i.ToListItemDto()).ToList(), total, query);
    }

    public async Task<DemandForecastDto> GetDemandForecastByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetDemandForecastByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Demand forecast not found.");
        await EnsureOwnedAsync(entity.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteDemandForecastAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetDemandForecastByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Demand forecast not found.");
        await EnsureOwnedAsync(entity.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);
        _repository.RemoveDemandForecast(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ================= Smart warehouse allocation =================

    public async Task<WarehouseAllocationRecommendationDto> RecommendWarehouseAsync(
        Guid currentUserId, bool isAdmin, RecommendWarehouseRequest request, CancellationToken cancellationToken)
    {
        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new ConflictException("You must have a logistics partner profile to get allocations.");

        var objective = ParseEnum<WarehouseAllocationObjective>(request.Objective ?? "Balanced", "Invalid Objective.");

        Guid? destinationDistrictId = request.DestinationDistrictId;
        Guid? shipmentId = null;
        if (request.ShipmentId.HasValue)
        {
            var shipment = await _repository.GetShipmentAsync(request.ShipmentId.Value, cancellationToken)
                ?? throw new ConflictException("Shipment not found.");
            if (!isAdmin && shipment.LogisticsPartnerProfileId != profile.Id)
            {
                throw new UnauthorizedAccessException("This shipment belongs to another logistics partner.");
            }

            shipmentId = shipment.Id;
            destinationDistrictId ??= shipment.DestinationDistrictId;
        }

        if (destinationDistrictId.HasValue
            && !await _repository.DistrictExistsAsync(destinationDistrictId.Value, cancellationToken))
        {
            throw new ConflictException("Destination district not found.");
        }

        var warehouses = await _repository.GetCandidateWarehousesAsync(profile.Id, cancellationToken);
        if (warehouses.Count == 0)
        {
            throw new ConflictException("You have no warehouses to allocate to.");
        }

        var result = _warehouseAllocationProvider.Recommend(new WarehouseAllocationInput
        {
            Objective = objective.ToString().ToLowerInvariant(),
            RequireColdChain = request.RequireColdChain,
            Quantity = request.Quantity,
            DestinationDistrictId = destinationDistrictId,
            Candidates = warehouses.Select(w => new WarehouseCandidate(
                w.Id, w.Code, w.Name, w.DistrictId, w.HasColdChain,
                w.TotalCapacityUnits, w.UsedCapacityUnits, w.Status.ToString())).ToList(),
        });

        var now = DateTime.UtcNow;
        var recommendation = new WarehouseAllocationRecommendation
        {
            Id = Guid.NewGuid(),
            LogisticsPartnerProfileId = profile.Id,
            GeneratedByUserId = currentUserId,
            Objective = objective,
            Sku = request.Sku?.Trim(),
            Quantity = request.Quantity,
            RequireColdChain = request.RequireColdChain,
            DestinationDistrictId = destinationDistrictId,
            ShipmentId = shipmentId,
            Method = result.Method,
            Summary = result.Summary,
            Confidence = result.Confidence,
            RecommendedWarehouseId = result.RecommendedWarehouseId,
            RecommendedWarehouseCode = result.RecommendedWarehouseId.HasValue
                ? warehouses.FirstOrDefault(w => w.Id == result.RecommendedWarehouseId.Value)?.Code
                : null,
            CreatedAt = now,
        };

        foreach (var o in result.Options)
        {
            recommendation.Options.Add(new WarehouseAllocationOption
            {
                Id = Guid.NewGuid(),
                WarehouseAllocationRecommendationId = recommendation.Id,
                WarehouseId = o.WarehouseId,
                WarehouseCode = o.Code,
                WarehouseName = o.Name,
                Rank = o.Rank,
                Score = o.Score,
                ProjectedUtilizationPercent = o.ProjectedUtilizationPercent,
                SameDistrictAsDestination = o.SameDistrictAsDestination,
                Rationale = o.Rationale,
            });
        }

        if (!request.Persist)
        {
            return recommendation.ToDto();
        }

        await _repository.AddWarehouseAllocationAsync(recommendation, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetWarehouseAllocationByIdAsync(recommendation.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<WarehouseAllocationRecommendationListItemDto>> GetWarehouseAllocationsAsync(
        Guid currentUserId, bool isAdmin, AiLogisticsQueryParameters query, CancellationToken cancellationToken)
    {
        NormalisePaging(query);
        var profileId = await ScopeProfileIdAsync(currentUserId, isAdmin, cancellationToken);
        var (items, total) = await _repository.GetWarehouseAllocationsPagedAsync(profileId, query, cancellationToken);
        return Page(items.Select(i => i.ToListItemDto()).ToList(), total, query);
    }

    public async Task<WarehouseAllocationRecommendationDto> GetWarehouseAllocationByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetWarehouseAllocationByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Warehouse allocation recommendation not found.");
        await EnsureOwnedAsync(entity.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteWarehouseAllocationAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetWarehouseAllocationByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Warehouse allocation recommendation not found.");
        await EnsureOwnedAsync(entity.LogisticsPartnerProfileId, currentUserId, isAdmin, cancellationToken);
        _repository.RemoveWarehouseAllocation(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ================= helpers =================

    /// <summary>The caller's own profile id, or null for an admin with no profile.</summary>
    private async Task<Guid?> ResolveProfileIdAsync(Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken);
        if (profile is null && !isAdmin)
        {
            throw new ConflictException("You must have a logistics partner profile to use AI Logistics.");
        }

        return profile?.Id;
    }

    /// <summary>Null when the caller is an admin (see everything), otherwise the caller's profile id.</summary>
    private async Task<Guid?> ScopeProfileIdAsync(Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (isAdmin)
        {
            return null;
        }

        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        return profile.Id;
    }

    private async Task EnsureOwnedAsync(
        Guid artefactProfileId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (isAdmin)
        {
            return;
        }

        var profile = await _partnerRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        if (artefactProfileId != profile.Id)
        {
            throw new UnauthorizedAccessException("This artefact belongs to another logistics partner.");
        }
    }

    private static Guid ResolveOwner(Guid entityProfileId, Guid? callerProfileId, bool isAdmin, string denyMessage)
    {
        if (isAdmin)
        {
            return callerProfileId ?? entityProfileId;
        }

        if (callerProfileId!.Value != entityProfileId)
        {
            throw new UnauthorizedAccessException(denyMessage);
        }

        return entityProfileId;
    }

    private static void NormalisePaging(AiLogisticsQueryParameters query)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;
    }

    private static PagedResult<T> Page<T>(List<T> items, int total, AiLogisticsQueryParameters query) => new()
    {
        Items = items,
        TotalCount = total,
        Page = query.Page,
        PageSize = query.PageSize,
    };

    private static decimal Round(double value) => Math.Round((decimal)value, 2);

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}

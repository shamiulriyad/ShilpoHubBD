using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Repositories;

public class AiLogisticsRepository : IAiLogisticsRepository
{
    private readonly ShilpoHubDbContext _context;

    public AiLogisticsRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ---- Signal gathering ------------------------------------------------

    public Task<Shipment?> GetShipmentAsync(Guid shipmentId, CancellationToken cancellationToken)
        => _context.Shipments
            .Include(s => s.OriginDistrict)
            .Include(s => s.DestinationDistrict)
            .FirstOrDefaultAsync(s => s.Id == shipmentId, cancellationToken);

    public async Task<LaneDeliveryStats> GetLaneStatsAsync(
        Guid profileId, Guid? originDistrictId, Guid? destinationDistrictId, DateTime sinceUtc,
        CancellationToken cancellationToken)
    {
        var query = _context.Shipments
            .Where(s => s.LogisticsPartnerProfileId == profileId && s.CreatedAt >= sinceUtc);

        if (originDistrictId.HasValue)
        {
            query = query.Where(s => s.OriginDistrictId == originDistrictId.Value);
        }

        if (destinationDistrictId.HasValue)
        {
            query = query.Where(s => s.DestinationDistrictId == destinationDistrictId.Value);
        }

        var rows = await query
            .Select(s => new
            {
                s.Status,
                s.DispatchedAt,
                s.DeliveredAt,
                s.EstimatedDeliveryAt,
                s.DeliveryAttemptCount,
            })
            .ToListAsync(cancellationToken);

        var delivered = rows
            .Where(r => r.Status == ShipmentStatus.Delivered && r.DispatchedAt.HasValue && r.DeliveredAt.HasValue)
            .ToList();

        double? avgTransit = delivered.Count > 0
            ? delivered.Average(r => (r.DeliveredAt!.Value - r.DispatchedAt!.Value).TotalDays)
            : null;

        var withPromise = delivered.Where(r => r.EstimatedDeliveryAt.HasValue).ToList();
        double? onTimeRate = withPromise.Count > 0
            ? withPromise.Count(r => r.DeliveredAt!.Value <= r.EstimatedDeliveryAt!.Value) / (double)withPromise.Count
            : null;

        var terminal = rows
            .Where(r => r.Status is ShipmentStatus.Delivered or ShipmentStatus.Returned or ShipmentStatus.DeliveryFailed)
            .ToList();
        double? failureRate = terminal.Count > 0
            ? terminal.Count(r => r.Status == ShipmentStatus.Returned || r.DeliveryAttemptCount >= 2) / (double)terminal.Count
            : null;

        return new LaneDeliveryStats(
            avgTransit is null ? null : Math.Round(avgTransit.Value, 3),
            onTimeRate is null ? null : Math.Round(onTimeRate.Value, 4),
            failureRate is null ? null : Math.Round(failureRate.Value, 4),
            delivered.Count);
    }

    public async Task<PartnerDeliveryStats> GetPartnerDeliveryStatsAsync(
        Guid profileId, DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var rows = await _context.Shipments
            .Where(s => s.LogisticsPartnerProfileId == profileId && s.CreatedAt >= sinceUtc)
            .Select(s => new
            {
                s.Status,
                s.DeliveredAt,
                s.EstimatedDeliveryAt,
                s.DeliveryAttemptCount,
            })
            .ToListAsync(cancellationToken);

        var delivered = rows.Where(r => r.Status == ShipmentStatus.Delivered).ToList();
        var withPromise = delivered.Where(r => r.EstimatedDeliveryAt.HasValue && r.DeliveredAt.HasValue).ToList();

        double? onTimeRate = withPromise.Count > 0
            ? withPromise.Count(r => r.DeliveredAt!.Value <= r.EstimatedDeliveryAt!.Value) / (double)withPromise.Count
            : null;

        double? avgAttempts = delivered.Count > 0
            ? delivered.Average(r => (double)Math.Max(1, r.DeliveryAttemptCount))
            : null;

        var terminal = rows
            .Where(r => r.Status is ShipmentStatus.Delivered or ShipmentStatus.Returned or ShipmentStatus.DeliveryFailed)
            .ToList();
        double? failureRate = terminal.Count > 0
            ? terminal.Count(r => r.Status == ShipmentStatus.Returned || r.DeliveryAttemptCount >= 2) / (double)terminal.Count
            : null;

        return new PartnerDeliveryStats(
            onTimeRate is null ? null : Math.Round(onTimeRate.Value, 4),
            avgAttempts is null ? null : Math.Round(avgAttempts.Value, 3),
            failureRate is null ? null : Math.Round(failureRate.Value, 4),
            delivered.Count);
    }

    public async Task<List<DemandDailyCount>> GetDailyDemandSeriesAsync(
        Guid profileId, DemandForecastScope scope, Guid? scopeId, string metric,
        DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        var m = metric.Trim().ToLowerInvariant();
        List<DemandDailyCount> series;

        if (scope == DemandForecastScope.Warehouse)
        {
            var type = m == "outbound" ? WarehouseStockMovementType.Outbound : WarehouseStockMovementType.Inbound;
            series = await _context.WarehouseStockMovements
                .Where(x => x.Warehouse.LogisticsPartnerProfileId == profileId
                    && x.WarehouseId == scopeId
                    && x.Type == type
                    && x.OccurredAt >= fromUtc && x.OccurredAt < toUtc)
                .GroupBy(x => x.OccurredAt.Date)
                .Select(g => new DemandDailyCount(g.Key, g.Sum(x => (double)x.Quantity)))
                .ToListAsync(cancellationToken);
            return series.OrderBy(p => p.Date).ToList();
        }

        Guid? districtId = scope == DemandForecastScope.District ? scopeId : null;

        switch (m)
        {
            case "pickups":
                series = await _context.PickupRequests
                    .Where(p => p.LogisticsPartnerProfileId == profileId
                        && p.CreatedAt >= fromUtc && p.CreatedAt < toUtc
                        && (districtId == null || p.OriginDistrictId == districtId))
                    .GroupBy(p => p.CreatedAt.Date)
                    .Select(g => new DemandDailyCount(g.Key, g.Count()))
                    .ToListAsync(cancellationToken);
                break;
            case "returns":
                series = await _context.ReturnRequests
                    .Where(r => r.LogisticsPartnerProfileId == profileId
                        && r.CreatedAt >= fromUtc && r.CreatedAt < toUtc
                        && (districtId == null || r.PickupDistrictId == districtId))
                    .GroupBy(r => r.CreatedAt.Date)
                    .Select(g => new DemandDailyCount(g.Key, g.Count()))
                    .ToListAsync(cancellationToken);
                break;
            case "weight_kg":
                series = await _context.Shipments
                    .Where(s => s.LogisticsPartnerProfileId == profileId
                        && s.CreatedAt >= fromUtc && s.CreatedAt < toUtc
                        && (districtId == null || s.DestinationDistrictId == districtId))
                    .GroupBy(s => s.CreatedAt.Date)
                    .Select(g => new DemandDailyCount(g.Key, g.Sum(s => (double?)s.TotalWeightKg) ?? 0))
                    .ToListAsync(cancellationToken);
                break;
            default: // shipments
                series = await _context.Shipments
                    .Where(s => s.LogisticsPartnerProfileId == profileId
                        && s.CreatedAt >= fromUtc && s.CreatedAt < toUtc
                        && (districtId == null || s.DestinationDistrictId == districtId))
                    .GroupBy(s => s.CreatedAt.Date)
                    .Select(g => new DemandDailyCount(g.Key, g.Count()))
                    .ToListAsync(cancellationToken);
                break;
        }

        return series.OrderBy(p => p.Date).ToList();
    }

    public Task<DeliveryRoute?> GetRouteWithStopsAsync(Guid routeId, CancellationToken cancellationToken)
        => _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.Id == routeId, cancellationToken);

    public async Task<List<Warehouse>> GetCandidateWarehousesAsync(Guid profileId, CancellationToken cancellationToken)
        => await _context.Warehouses
            .Where(w => w.LogisticsPartnerProfileId == profileId)
            .ToListAsync(cancellationToken);

    // ---- Reference checks -----------------------------------------------

    public Task<bool> ShipmentBelongsToProfileAsync(Guid shipmentId, Guid profileId, CancellationToken cancellationToken)
        => _context.Shipments.AnyAsync(s => s.Id == shipmentId && s.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task<bool> RouteBelongsToProfileAsync(Guid routeId, Guid profileId, CancellationToken cancellationToken)
        => _context.DeliveryRoutes.AnyAsync(r => r.Id == routeId && r.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task<bool> WarehouseBelongsToProfileAsync(Guid warehouseId, Guid profileId, CancellationToken cancellationToken)
        => _context.Warehouses.AnyAsync(w => w.Id == warehouseId && w.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);

    public Task<string?> GetDistrictNameAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.Where(d => d.Id == districtId).Select(d => d.Name).FirstOrDefaultAsync(cancellationToken);

    public Task<string?> GetWarehouseLabelAsync(Guid warehouseId, CancellationToken cancellationToken)
        => _context.Warehouses.Where(w => w.Id == warehouseId)
            .Select(w => w.Code + " - " + w.Name)
            .FirstOrDefaultAsync(cancellationToken);

    // ---- Delivery predictions ------------------------------------------

    public async Task AddDeliveryPredictionAsync(DeliveryPrediction prediction, CancellationToken cancellationToken)
        => await _context.DeliveryPredictions.AddAsync(prediction, cancellationToken);

    public void RemoveDeliveryPrediction(DeliveryPrediction prediction)
        => _context.DeliveryPredictions.Remove(prediction);

    public Task<DeliveryPrediction?> GetDeliveryPredictionByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.DeliveryPredictions
            .Include(p => p.Shipment)
            .Include(p => p.GeneratedBy)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(List<DeliveryPrediction> Items, int TotalCount)> GetDeliveryPredictionsPagedAsync(
        Guid? profileId, AiLogisticsQueryParameters query, CancellationToken cancellationToken)
    {
        var items = _context.DeliveryPredictions.Include(p => p.Shipment).AsQueryable();

        if (profileId.HasValue)
        {
            items = items.Where(p => p.LogisticsPartnerProfileId == profileId.Value);
        }

        if (query.ShipmentId.HasValue)
        {
            items = items.Where(p => p.ShipmentId == query.ShipmentId.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(query.CreatedFrom.Value, DateTimeKind.Utc);
            items = items.Where(p => p.CreatedAt >= from);
        }

        if (query.CreatedTo.HasValue)
        {
            var to = DateTime.SpecifyKind(query.CreatedTo.Value, DateTimeKind.Utc);
            items = items.Where(p => p.CreatedAt <= to);
        }

        items = items.OrderByDescending(p => p.CreatedAt);
        return await PageAsync(items, query, cancellationToken);
    }

    // ---- Route optimization runs -------------------------------------

    public async Task AddRouteOptimizationRunAsync(RouteOptimizationRun run, CancellationToken cancellationToken)
        => await _context.RouteOptimizationRuns.AddAsync(run, cancellationToken);

    public void RemoveRouteOptimizationRun(RouteOptimizationRun run)
        => _context.RouteOptimizationRuns.Remove(run);

    public Task<RouteOptimizationRun?> GetRouteOptimizationRunByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.RouteOptimizationRuns
            .Include(r => r.DeliveryRoute)
            .Include(r => r.GeneratedBy)
            .Include(r => r.Stops)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(List<RouteOptimizationRun> Items, int TotalCount)> GetRouteOptimizationRunsPagedAsync(
        Guid? profileId, AiLogisticsQueryParameters query, CancellationToken cancellationToken)
    {
        var items = _context.RouteOptimizationRuns.Include(r => r.DeliveryRoute).AsQueryable();

        if (profileId.HasValue)
        {
            items = items.Where(r => r.LogisticsPartnerProfileId == profileId.Value);
        }

        if (query.DeliveryRouteId.HasValue)
        {
            items = items.Where(r => r.DeliveryRouteId == query.DeliveryRouteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<RouteOptimizationRunStatus>(query.Status, true, out var status))
        {
            items = items.Where(r => r.Status == status);
        }

        if (query.CreatedFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(query.CreatedFrom.Value, DateTimeKind.Utc);
            items = items.Where(r => r.CreatedAt >= from);
        }

        if (query.CreatedTo.HasValue)
        {
            var to = DateTime.SpecifyKind(query.CreatedTo.Value, DateTimeKind.Utc);
            items = items.Where(r => r.CreatedAt <= to);
        }

        items = items.OrderByDescending(r => r.CreatedAt);
        return await PageAsync(items, query, cancellationToken);
    }

    // ---- Demand forecasts -----------------------------------------

    public async Task AddDemandForecastAsync(DemandForecast forecast, CancellationToken cancellationToken)
        => await _context.LogisticsDemandForecasts.AddAsync(forecast, cancellationToken);

    public void RemoveDemandForecast(DemandForecast forecast)
        => _context.LogisticsDemandForecasts.Remove(forecast);

    public Task<DemandForecast?> GetDemandForecastByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.LogisticsDemandForecasts
            .Include(f => f.GeneratedBy)
            .Include(f => f.Points)
            .AsSplitQuery()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<(List<DemandForecast> Items, int TotalCount)> GetDemandForecastsPagedAsync(
        Guid? profileId, AiLogisticsQueryParameters query, CancellationToken cancellationToken)
    {
        var items = _context.LogisticsDemandForecasts.AsQueryable();

        if (profileId.HasValue)
        {
            items = items.Where(f => f.LogisticsPartnerProfileId == profileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Scope)
            && Enum.TryParse<DemandForecastScope>(query.Scope, true, out var scope))
        {
            items = items.Where(f => f.Scope == scope);
        }

        if (query.CreatedFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(query.CreatedFrom.Value, DateTimeKind.Utc);
            items = items.Where(f => f.CreatedAt >= from);
        }

        if (query.CreatedTo.HasValue)
        {
            var to = DateTime.SpecifyKind(query.CreatedTo.Value, DateTimeKind.Utc);
            items = items.Where(f => f.CreatedAt <= to);
        }

        items = items.OrderByDescending(f => f.CreatedAt);
        return await PageAsync(items, query, cancellationToken);
    }

    // ---- Warehouse allocation recommendations ------------------

    public async Task AddWarehouseAllocationAsync(
        WarehouseAllocationRecommendation recommendation, CancellationToken cancellationToken)
        => await _context.WarehouseAllocationRecommendations.AddAsync(recommendation, cancellationToken);

    public void RemoveWarehouseAllocation(WarehouseAllocationRecommendation recommendation)
        => _context.WarehouseAllocationRecommendations.Remove(recommendation);

    public Task<WarehouseAllocationRecommendation?> GetWarehouseAllocationByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.WarehouseAllocationRecommendations
            .Include(r => r.GeneratedBy)
            .Include(r => r.DestinationDistrict)
            .Include(r => r.Shipment)
            .Include(r => r.RecommendedWarehouse)
            .Include(r => r.Options)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(List<WarehouseAllocationRecommendation> Items, int TotalCount)> GetWarehouseAllocationsPagedAsync(
        Guid? profileId, AiLogisticsQueryParameters query, CancellationToken cancellationToken)
    {
        var items = _context.WarehouseAllocationRecommendations
            .Include(r => r.RecommendedWarehouse)
            .Include(r => r.Options)
            .AsQueryable();

        if (profileId.HasValue)
        {
            items = items.Where(r => r.LogisticsPartnerProfileId == profileId.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(query.CreatedFrom.Value, DateTimeKind.Utc);
            items = items.Where(r => r.CreatedAt >= from);
        }

        if (query.CreatedTo.HasValue)
        {
            var to = DateTime.SpecifyKind(query.CreatedTo.Value, DateTimeKind.Utc);
            items = items.Where(r => r.CreatedAt <= to);
        }

        items = items.OrderByDescending(r => r.CreatedAt);
        return await PageAsync(items, query, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    // ---- helpers ----------------------------------------------

    private static async Task<(List<T> Items, int TotalCount)> PageAsync<T>(
        IQueryable<T> query, AiLogisticsQueryParameters q, CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}

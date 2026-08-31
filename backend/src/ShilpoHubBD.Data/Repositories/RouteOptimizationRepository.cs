using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Repositories;

public class RouteOptimizationRepository : IRouteOptimizationRepository
{
    private readonly ShilpoHubDbContext _context;

    public RouteOptimizationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DeliveryRoute route, CancellationToken cancellationToken)
        => await _context.DeliveryRoutes.AddAsync(route, cancellationToken);

    public void Remove(DeliveryRoute route) => _context.DeliveryRoutes.Remove(route);

    public Task<DeliveryRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.DeliveryRoutes
            .Include(r => r.Profile)
            .Include(r => r.CreatedBy)
            .Include(r => r.OriginDistrict)
            .Include(r => r.Stops).ThenInclude(s => s.District)
            .Include(r => r.Stops).ThenInclude(s => s.PickupRequest)
            .Include(r => r.Stops).ThenInclude(s => s.Order)
            .Include(r => r.Events).ThenInclude(e => e.Actor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> RouteCodeExistsAsync(string routeCode, CancellationToken cancellationToken)
        => _context.DeliveryRoutes.AnyAsync(r => r.RouteCode == routeCode, cancellationToken);

    public async Task<(List<DeliveryRoute> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, DeliveryRouteQueryParameters query, CancellationToken cancellationToken)
    {
        var routes = _context.DeliveryRoutes.AsQueryable();

        if (profileId.HasValue)
        {
            routes = routes.Where(r => r.LogisticsPartnerProfileId == profileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<DeliveryRouteStatus>(query.Status, true, out var status))
        {
            routes = routes.Where(r => r.Status == status);
        }

        if (query.ScheduledFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(query.ScheduledFrom.Value, DateTimeKind.Utc);
            routes = routes.Where(r => r.ScheduledDate >= from);
        }

        if (query.ScheduledTo.HasValue)
        {
            var to = DateTime.SpecifyKind(query.ScheduledTo.Value, DateTimeKind.Utc);
            routes = routes.Where(r => r.ScheduledDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            routes = routes.Where(r =>
                r.RouteCode.ToLower().Contains(term)
                || r.Name.ToLower().Contains(term)
                || (r.AssignedDriverName != null && r.AssignedDriverName.ToLower().Contains(term)));
        }

        routes = routes
            .OrderByDescending(r => r.Status == DeliveryRouteStatus.InProgress || r.Status == DeliveryRouteStatus.Dispatched)
            .ThenBy(r => r.ScheduledDate ?? DateTime.MaxValue)
            .ThenByDescending(r => r.CreatedAt);

        var totalCount = await routes.CountAsync(cancellationToken);
        var items = await routes
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);

    public Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken)
        => _context.Orders.AnyAsync(o => o.Id == orderId, cancellationToken);

    public Task<bool> PickupRequestBelongsToProfileAsync(
        Guid pickupRequestId, Guid profileId, CancellationToken cancellationToken)
        => _context.PickupRequests.AnyAsync(
            p => p.Id == pickupRequestId && p.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageRouteRepository : IHeritageRouteRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageRouteRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<HeritageRoute> WithDetails()
        => _context.HeritageRoutes
            .Include(r => r.Stops.OrderBy(s => s.Order))
            .ThenInclude(s => s.HeritagePlace)
            .AsSplitQuery();

    public async Task<(List<HeritageRoute> Items, int TotalCount)> GetPagedAsync(HeritageRouteQueryParameters query, CancellationToken cancellationToken)
    {
        var routes = WithDetails().AsQueryable();

        if (query.Status.HasValue)
        {
            routes = routes.Where(r => r.Status == query.Status.Value);
        }

        if (query.IsRecommended.HasValue)
        {
            routes = routes.Where(r => r.IsRecommended == query.IsRecommended.Value);
        }

        if (query.DistrictId.HasValue)
        {
            routes = routes.Where(r => r.Stops.Any(s => s.HeritagePlace.DistrictId == query.DistrictId.Value));
        }

        routes = routes.OrderByDescending(r => r.IsRecommended).ThenBy(r => r.Name);

        var totalCount = await routes.CountAsync(cancellationToken);
        var items = await routes
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<HeritageRoute>> GetRecommendedAsync(CancellationToken cancellationToken)
        => WithDetails()
            .Where(r => r.IsRecommended && r.Status == RouteStatus.Published)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public Task<HeritageRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddAsync(HeritageRoute route, CancellationToken cancellationToken)
        => await _context.HeritageRoutes.AddAsync(route, cancellationToken);

    public void Remove(HeritageRoute route)
        => _context.HeritageRoutes.Remove(route);

    public async Task AddStopAsync(RouteStop stop, CancellationToken cancellationToken)
        => await _context.RouteStops.AddAsync(stop, cancellationToken);

    public void RemoveStop(RouteStop stop)
        => _context.RouteStops.Remove(stop);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

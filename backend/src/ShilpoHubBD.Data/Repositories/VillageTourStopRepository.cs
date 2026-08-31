using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Data.Repositories;

public class VillageTourStopRepository : IVillageTourStopRepository
{
    private readonly ShilpoHubDbContext _context;

    public VillageTourStopRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<VillageTourStop> WithDetails()
        => _context.VillageTourStops.Include(s => s.HeritagePlace);

    public async Task<(List<VillageTourStop> Items, int TotalCount)> GetPagedAsync(
        VillageTourStopQueryParameters query, CancellationToken cancellationToken)
    {
        var stops = WithDetails().Where(s => s.IsActive);

        if (query.HeritagePlaceId.HasValue)
        {
            stops = stops.Where(s => s.HeritagePlaceId == query.HeritagePlaceId.Value);
        }

        stops = stops.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Title);

        var totalCount = await stops.CountAsync(cancellationToken);
        var items = await stops
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<VillageTourStop?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(VillageTourStop stop, CancellationToken cancellationToken)
        => await _context.VillageTourStops.AddAsync(stop, cancellationToken);

    public void Remove(VillageTourStop stop)
        => _context.VillageTourStops.Remove(stop);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

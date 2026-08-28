using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Repositories;

public class CulturalEventRepository : ICulturalEventRepository
{
    private readonly ShilpoHubDbContext _context;

    public CulturalEventRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<CulturalEvent> WithDetails()
        => _context.CulturalEvents.Include(e => e.District).Include(e => e.HeritagePlace).AsSplitQuery();

    public async Task<(List<CulturalEvent> Items, int TotalCount)> GetPagedAsync(CulturalEventQueryParameters query, CancellationToken cancellationToken)
    {
        var events = WithDetails();

        if (query.ActiveOnly)
        {
            events = events.Where(e => e.IsActive);
        }

        if (query.DistrictId.HasValue)
        {
            events = events.Where(e => e.DistrictId == query.DistrictId.Value);
        }

        if (query.HeritagePlaceId.HasValue)
        {
            events = events.Where(e => e.HeritagePlaceId == query.HeritagePlaceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            events = events.Where(e => e.Category == query.Category);
        }

        if (query.FromDate.HasValue)
        {
            events = events.Where(e => (e.EndDate ?? e.EventDate) >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            events = events.Where(e => e.EventDate <= query.ToDate.Value);
        }

        events = events.OrderBy(e => e.EventDate);

        var totalCount = await events.CountAsync(cancellationToken);
        var items = await events
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<CulturalEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(CulturalEvent culturalEvent, CancellationToken cancellationToken)
        => await _context.CulturalEvents.AddAsync(culturalEvent, cancellationToken);

    public void Remove(CulturalEvent culturalEvent)
        => _context.CulturalEvents.Remove(culturalEvent);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Repositories;

public class CmsEventRepository : ICmsEventRepository
{
    private readonly ShilpoHubDbContext _context;

    public CmsEventRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<(List<CmsEvent> Items, int TotalCount)> GetPagedAsync(
        CmsEventQueryParameters query, bool publishedOnly, CancellationToken cancellationToken)
    {
        var events = _context.CmsEvents.AsQueryable();

        if (publishedOnly)
        {
            events = events.Where(e => e.IsPublished);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            events = events.Where(e => EF.Functions.ILike(e.Title, term) || EF.Functions.ILike(e.Description, term));
        }

        if (query.FromDate.HasValue)
        {
            events = events.Where(e => e.EndDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            events = events.Where(e => e.StartDate <= query.ToDate.Value);
        }

        events = events.OrderBy(e => e.StartDate);

        var totalCount = await events.CountAsync(cancellationToken);
        var items = await events
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<CmsEvent> Items, int TotalCount)> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var events = _context.CmsEvents.Where(e => !e.IsPublished).OrderByDescending(e => e.CreatedAt);

        var totalCount = await events.CountAsync(cancellationToken);
        var items = await events.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<CmsEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.CmsEvents.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<CmsEvent?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => _context.CmsEvents.FirstOrDefaultAsync(e => e.Slug == slug, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
        => _context.CmsEvents.AnyAsync(e => e.Slug == slug, cancellationToken);

    public async Task AddAsync(CmsEvent cmsEvent, CancellationToken cancellationToken)
        => await _context.CmsEvents.AddAsync(cmsEvent, cancellationToken);

    public void Remove(CmsEvent cmsEvent)
        => _context.CmsEvents.Remove(cmsEvent);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

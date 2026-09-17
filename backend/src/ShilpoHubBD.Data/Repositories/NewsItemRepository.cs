using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Repositories;

public class NewsItemRepository : INewsItemRepository
{
    private readonly ShilpoHubDbContext _context;

    public NewsItemRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<(List<NewsItem> Items, int TotalCount)> GetPagedAsync(
        NewsItemQueryParameters query, bool publishedOnly, CancellationToken cancellationToken)
    {
        var items = _context.NewsItems.AsQueryable();

        if (publishedOnly)
        {
            items = items.Where(n => n.IsPublished);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            items = items.Where(n => EF.Functions.ILike(n.Title, term) || EF.Functions.ILike(n.Summary, term));
        }

        items = items.OrderByDescending(n => n.PublishedAt ?? n.CreatedAt);

        var totalCount = await items.CountAsync(cancellationToken);
        var page = await items
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (page, totalCount);
    }

    public async Task<(List<NewsItem> Items, int TotalCount)> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var items = _context.NewsItems.Where(n => !n.IsPublished).OrderByDescending(n => n.CreatedAt);

        var totalCount = await items.CountAsync(cancellationToken);
        var results = await items.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (results, totalCount);
    }

    public Task<NewsItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.NewsItems.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<NewsItem?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => _context.NewsItems.FirstOrDefaultAsync(n => n.Slug == slug, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
        => _context.NewsItems.AnyAsync(n => n.Slug == slug, cancellationToken);

    public async Task AddAsync(NewsItem item, CancellationToken cancellationToken)
        => await _context.NewsItems.AddAsync(item, cancellationToken);

    public void Remove(NewsItem item)
        => _context.NewsItems.Remove(item);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

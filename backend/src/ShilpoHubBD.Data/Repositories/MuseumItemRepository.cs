using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Data.Repositories;

public class MuseumItemRepository : IMuseumItemRepository
{
    private readonly ShilpoHubDbContext _context;

    public MuseumItemRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<MuseumItem> WithDetails()
        => _context.MuseumItems
            .Include(m => m.District)
            .Include(m => m.Product)
            .Include(m => m.Media)
            .AsSplitQuery();

    public async Task<(List<MuseumItem> Items, int TotalCount)> GetPagedAsync(MuseumItemQueryParameters query, CancellationToken cancellationToken)
    {
        var items = WithDetails().Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            items = items.Where(m => EF.Functions.ILike(m.Title, $"%{search}%") || EF.Functions.ILike(m.Description, $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            items = items.Where(m => m.Category == query.Category);
        }

        if (query.DistrictId.HasValue)
        {
            items = items.Where(m => m.DistrictId == query.DistrictId.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            items = items.Where(m => m.IsFeatured == query.IsFeatured.Value);
        }

        items = items.OrderByDescending(m => m.IsFeatured).ThenBy(m => m.Title);

        var totalCount = await items.CountAsync(cancellationToken);
        var page = await items
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (page, totalCount);
    }

    public Task<MuseumItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task AddAsync(MuseumItem item, CancellationToken cancellationToken)
        => await _context.MuseumItems.AddAsync(item, cancellationToken);

    public void Remove(MuseumItem item)
        => _context.MuseumItems.Remove(item);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

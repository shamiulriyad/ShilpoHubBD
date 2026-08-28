using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Repositories;

public class LocalCuisineRepository : ILocalCuisineRepository
{
    private readonly ShilpoHubDbContext _context;

    public LocalCuisineRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<LocalCuisine> WithDetails()
        => _context.LocalCuisines.Include(c => c.District).Include(c => c.HeritagePlace).AsSplitQuery();

    public async Task<(List<LocalCuisine> Items, int TotalCount)> GetPagedAsync(LocalCuisineQueryParameters query, CancellationToken cancellationToken)
    {
        var cuisines = WithDetails().Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            cuisines = cuisines.Where(c => EF.Functions.ILike(c.Name, $"%{search}%") || EF.Functions.ILike(c.Description, $"%{search}%"));
        }

        if (query.DistrictId.HasValue)
        {
            cuisines = cuisines.Where(c => c.DistrictId == query.DistrictId.Value);
        }

        if (query.HeritagePlaceId.HasValue)
        {
            cuisines = cuisines.Where(c => c.HeritagePlaceId == query.HeritagePlaceId.Value);
        }

        cuisines = cuisines.OrderBy(c => c.Name);

        var totalCount = await cuisines.CountAsync(cancellationToken);
        var items = await cuisines
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<LocalCuisine?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(LocalCuisine cuisine, CancellationToken cancellationToken)
        => await _context.LocalCuisines.AddAsync(cuisine, cancellationToken);

    public void Remove(LocalCuisine cuisine)
        => _context.LocalCuisines.Remove(cuisine);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Repositories;

public class HeritagePlaceRepository : IHeritagePlaceRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritagePlaceRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<HeritagePlace> WithDetails()
        => _context.HeritagePlaces.Include(p => p.District).AsSplitQuery();

    public async Task<(List<HeritagePlace> Items, int TotalCount)> GetPagedAsync(HeritagePlaceQueryParameters query, CancellationToken cancellationToken)
    {
        var places = WithDetails().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            places = places.Where(p => EF.Functions.ILike(p.Name, $"%{search}%") || EF.Functions.ILike(p.Description, $"%{search}%"));
        }

        if (query.DistrictId.HasValue)
        {
            places = places.Where(p => p.DistrictId == query.DistrictId.Value);
        }

        if (query.PlaceType.HasValue)
        {
            places = places.Where(p => p.PlaceType == query.PlaceType.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            places = places.Where(p => p.IsFeatured == query.IsFeatured.Value);
        }

        places = places.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Name);

        var totalCount = await places.CountAsync(cancellationToken);
        var items = await places
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<HeritagePlace>> GetActiveWithinBoundsAsync(
        double minLatitude, double maxLatitude, double minLongitude, double maxLongitude, CancellationToken cancellationToken)
        => await WithDetails()
            .Where(p => p.IsActive
                && p.Latitude >= minLatitude && p.Latitude <= maxLatitude
                && p.Longitude >= minLongitude && p.Longitude <= maxLongitude)
            .ToListAsync(cancellationToken);

    public Task<HeritagePlace?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(HeritagePlace place, CancellationToken cancellationToken)
        => await _context.HeritagePlaces.AddAsync(place, cancellationToken);

    public void Remove(HeritagePlace place)
        => _context.HeritagePlaces.Remove(place);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

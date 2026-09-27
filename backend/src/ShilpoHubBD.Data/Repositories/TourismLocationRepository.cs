using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Data.Repositories;

public class TourismLocationRepository : ITourismLocationRepository
{
    private readonly ShilpoHubDbContext _context;

    public TourismLocationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<TourismLocation> WithDetails()
        => _context.TourismLocations.Include(l => l.District);

    public async Task<(List<TourismLocation> Items, int TotalCount)> GetPagedAsync(TourismLocationQueryParameters query, CancellationToken cancellationToken)
    {
        var locations = WithDetails();

        if (query.IsActive.HasValue)
        {
            locations = locations.Where(l => l.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            locations = locations.Where(l => EF.Functions.ILike(l.Name, $"%{search}%") || EF.Functions.ILike(l.Description, $"%{search}%"));
        }

        if (query.DistrictId.HasValue)
        {
            locations = locations.Where(l => l.DistrictId == query.DistrictId.Value);
        }

        if (query.Type.HasValue)
        {
            locations = locations.Where(l => l.Type == query.Type.Value);
        }

        if (query.IsVerified.HasValue)
        {
            locations = locations.Where(l => l.IsVerified == query.IsVerified.Value);
        }

        locations = locations.OrderBy(l => l.Source == "OpenStreetMap").ThenByDescending(l => l.IsVerified).ThenBy(l => l.Name);

        var totalCount = await locations.CountAsync(cancellationToken);
        var items = await locations
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<TourismLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<List<TourismLocation>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        => WithDetails().Where(l => ids.Contains(l.Id)).ToListAsync(cancellationToken);

    public async Task<List<TourismLocation>> GetForDistrictAsync(Guid districtId, IReadOnlyCollection<TourismLocationType> types, CancellationToken cancellationToken)
        => await WithDetails()
            .Where(l => l.DistrictId == districtId && l.IsActive && types.Contains(l.Type))
            .OrderBy(l => l.Source == "OpenStreetMap").ThenByDescending(l => l.IsVerified).ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<TourismLocation>> GetInBoundsAsync(double minLat, double maxLat, double minLon, double maxLon, IReadOnlyCollection<TourismLocationType> types, CancellationToken cancellationToken)
        => await WithDetails()
            .Where(l => l.IsActive && types.Contains(l.Type)
                && l.Latitude >= minLat && l.Latitude <= maxLat && l.Longitude >= minLon && l.Longitude <= maxLon)
            .ToListAsync(cancellationToken);

    public Task<List<TourismLocation>> GetAllForDistrictTrackedAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.TourismLocations.Where(l => l.DistrictId == districtId).ToListAsync(cancellationToken);

    public Task<DateTime?> GetLastSyncedAtAsync(Guid districtId, IReadOnlyCollection<TourismLocationType>? types, CancellationToken cancellationToken)
        => _context.TourismLocations
            .Where(l => l.DistrictId == districtId && l.Source == "OpenStreetMap" && (types == null || types.Contains(l.Type)))
            .MaxAsync(l => l.LastSyncedAt, cancellationToken);

    public async Task AddAsync(TourismLocation location, CancellationToken cancellationToken)
        => await _context.TourismLocations.AddAsync(location, cancellationToken);

    public void Remove(TourismLocation location)
        => _context.TourismLocations.Remove(location);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

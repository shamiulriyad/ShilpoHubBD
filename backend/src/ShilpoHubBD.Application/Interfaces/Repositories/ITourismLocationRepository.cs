using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ITourismLocationRepository
{
    Task<(List<TourismLocation> Items, int TotalCount)> GetPagedAsync(TourismLocationQueryParameters query, CancellationToken cancellationToken);
    Task<TourismLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TourismLocation>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    // Rows of the given types in one district, admin-entered ones first.
    Task<List<TourismLocation>> GetForDistrictAsync(Guid districtId, IReadOnlyCollection<TourismLocationType> types, CancellationToken cancellationToken);
    // Active rows of the given types inside a bounding box (callers narrow it to a true radius).
    Task<List<TourismLocation>> GetInBoundsAsync(double minLat, double maxLat, double minLon, double maxLon, IReadOnlyCollection<TourismLocationType> types, CancellationToken cancellationToken);
    // Every row of a district, tracked, for the import to update in place.
    Task<List<TourismLocation>> GetAllForDistrictTrackedAsync(Guid districtId, CancellationToken cancellationToken);
    Task<DateTime?> GetLastSyncedAtAsync(Guid districtId, IReadOnlyCollection<TourismLocationType>? types, CancellationToken cancellationToken);
    Task AddAsync(TourismLocation location, CancellationToken cancellationToken);
    void Remove(TourismLocation location);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

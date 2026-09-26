using ShilpoHubBD.Application.DTOs.Tourism;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Fetches and normalises points of interest around a coordinate from an external source
// (OpenStreetMap Overpass). Throws on failure so the caller can decide to serve stored data.
public interface IExternalPoiClient
{
    // Runs one small Overpass query per group (accommodation / food / sights / worship); a group that
    // fails is skipped, and the call throws only if every group failed.
    Task<List<ExternalPoiDto>> FetchAsync(double latitude, double longitude, double radiusKm, ExternalPoiScope scope, CancellationToken cancellationToken);
}

// Keeps a district's imported OpenStreetMap rows in the tourism database: refresh when stale,
// deduplicate against admin-entered records, never break the caller.
public interface ITourismExternalSyncService
{
    Task<TourismSyncResultDto> EnsureFreshAsync(Guid districtId, CancellationToken cancellationToken, double? radiusKm = null, ExternalPoiScope scope = ExternalPoiScope.All);
    Task<TourismSyncResultDto> SyncAsync(Guid districtId, bool force, CancellationToken cancellationToken, double? radiusKm = null, ExternalPoiScope scope = ExternalPoiScope.All);
}

public interface IAccommodationService
{
    Task<List<TourismLocationDto>> GetByDistrictAsync(Guid districtId, CancellationToken cancellationToken);
    Task<List<TourismLocationDto>> GetNearbyAsync(double latitude, double longitude, double? radiusKm, CancellationToken cancellationToken);
    // Everything to stay at within radiusKm of the destination's coordinates, nearest first.
    Task<NearbyAccommodationsDto> GetAroundDestinationAsync(Guid districtId, double? radiusKm, CancellationToken cancellationToken);
}

public interface ITourismPoiService
{
    Task<List<TourismLocationDto>> GetByDistrictAsync(Guid districtId, CancellationToken cancellationToken);
    Task<List<TourismLocationDto>> GetNearbyAsync(double latitude, double longitude, double? radiusKm, CancellationToken cancellationToken);
}

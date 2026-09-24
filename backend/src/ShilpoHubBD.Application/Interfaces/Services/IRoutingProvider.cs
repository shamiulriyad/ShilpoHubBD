using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Real road-network routing between two known points. Returns null if the routing engine is
// unavailable or returns no route -- callers must never substitute a straight-line guess for a
// missing result.
public interface IRoutingProvider
{
    Task<RouteResultDto?> GetDrivingRouteAsync(GeoPointDto origin, GeoPointDto destination, CancellationToken cancellationToken);
}

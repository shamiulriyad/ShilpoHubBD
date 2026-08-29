using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IRouteOptimizationRepository
{
    Task AddAsync(DeliveryRoute route, CancellationToken cancellationToken);

    void Remove(DeliveryRoute route);

    Task<DeliveryRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> RouteCodeExistsAsync(string routeCode, CancellationToken cancellationToken);

    Task<(List<DeliveryRoute> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, DeliveryRouteQueryParameters query, CancellationToken cancellationToken);

    Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken);

    Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>True when the pickup request exists and belongs to the given partner profile.</summary>
    Task<bool> PickupRequestBelongsToProfileAsync(
        Guid pickupRequestId, Guid profileId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

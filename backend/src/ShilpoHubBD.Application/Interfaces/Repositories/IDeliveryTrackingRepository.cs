using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IDeliveryTrackingRepository
{
    Task AddAsync(Shipment shipment, CancellationToken cancellationToken);

    void Remove(Shipment shipment);

    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken);

    Task<bool> TrackingNumberExistsAsync(string trackingNumber, CancellationToken cancellationToken);

    Task<(List<Shipment> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, ShipmentQueryParameters query, CancellationToken cancellationToken);

    Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken);

    Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken);

    Task<bool> PickupRequestBelongsToProfileAsync(
        Guid pickupRequestId, Guid profileId, CancellationToken cancellationToken);

    Task<bool> RouteBelongsToProfileAsync(Guid routeId, Guid profileId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

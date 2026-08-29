using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IReturnHandlingRepository
{
    Task AddAsync(ReturnRequest returnRequest, CancellationToken cancellationToken);

    void Remove(ReturnRequest returnRequest);

    Task<ReturnRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ReferenceExistsAsync(string referenceCode, CancellationToken cancellationToken);

    Task<(List<ReturnRequest> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, ReturnRequestQueryParameters query, CancellationToken cancellationToken);

    Task<bool> ShipmentBelongsToProfileAsync(Guid shipmentId, Guid profileId, CancellationToken cancellationToken);

    Task<bool> WarehouseBelongsToProfileAsync(Guid warehouseId, Guid profileId, CancellationToken cancellationToken);

    Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken);

    Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken);

    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

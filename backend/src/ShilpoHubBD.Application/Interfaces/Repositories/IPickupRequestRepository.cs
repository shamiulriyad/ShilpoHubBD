using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPickupRequestRepository
{
    Task AddAsync(PickupRequest request, CancellationToken cancellationToken);

    void Remove(PickupRequest request);

    Task<PickupRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ReferenceExistsAsync(string referenceCode, CancellationToken cancellationToken);

    Task<(List<PickupRequest> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, PickupRequestQueryParameters query, CancellationToken cancellationToken);

    Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken);

    Task<Domain.Entities.Marketplace.District?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ILogisticsPartnerRepository
{
    Task AddAsync(LogisticsPartnerProfile profile, CancellationToken cancellationToken);

    void Remove(LogisticsPartnerProfile profile);

    Task<LogisticsPartnerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<LogisticsPartnerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<(List<LogisticsPartnerProfile> Items, int TotalCount)> GetPagedAsync(
        LogisticsPartnerQueryParameters query, CancellationToken cancellationToken);

    Task<bool> HasPickupRequestsAsync(Guid profileId, CancellationToken cancellationToken);

    Task<Domain.Entities.Marketplace.District?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken);

    Task<bool> UserInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

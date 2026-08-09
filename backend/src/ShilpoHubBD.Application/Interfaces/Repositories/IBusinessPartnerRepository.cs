using ShilpoHubBD.Application.DTOs.BusinessPartner;
using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IBusinessPartnerRepository
{
    Task<BusinessPartnerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber, Guid? excludeProfileId, CancellationToken cancellationToken);
    Task<(List<BusinessPartnerProfile> Items, int TotalCount)> GetPagedAsync(BusinessPartnerQueryParameters parameters, CancellationToken cancellationToken);
    Task AddAsync(BusinessPartnerProfile profile, CancellationToken cancellationToken);
    void Remove(BusinessPartnerProfile profile);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

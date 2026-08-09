using ShilpoHubBD.Application.DTOs.BusinessPartner;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IBusinessPartnerService
{
    Task<BusinessPartnerProfileDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<PagedResult<BusinessPartnerProfileDto>> GetPagedAsync(BusinessPartnerQueryParameters parameters, CancellationToken cancellationToken);
    Task<BusinessPartnerProfileDto> UpsertAsync(Guid userId, Guid currentUserId, bool isAdmin, UpsertBusinessPartnerProfileRequest request, CancellationToken cancellationToken);
    Task<BusinessPartnerProfileDto> VerifyAsync(Guid userId, Guid verifierUserId, VerifyBusinessPartnerRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}

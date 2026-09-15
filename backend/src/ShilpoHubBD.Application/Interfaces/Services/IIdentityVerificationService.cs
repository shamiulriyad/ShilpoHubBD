using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IIdentityVerificationService
{
    Task<IdentityVerificationDto> SubmitAsync(
        Guid userId, SubmitIdentityVerificationRequest request, CancellationToken cancellationToken);

    Task<List<IdentityVerificationDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken);

    Task<PagedResult<IdentityVerificationDto>> GetPagedAsync(
        IdentityVerificationQueryParameters query, CancellationToken cancellationToken);

    Task<IdentityVerificationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IdentityVerificationDto> ApproveAsync(Guid id, Guid reviewerUserId, CancellationToken cancellationToken);

    Task<IdentityVerificationDto> RejectAsync(
        Guid id, Guid reviewerUserId, RejectIdentityVerificationRequest request, CancellationToken cancellationToken);
}

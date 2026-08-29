using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ILogisticsPartnerService
{
    Task<LogisticsPartnerProfileDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<LogisticsPartnerProfileDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<LogisticsPartnerProfileListItemDto>> GetPagedAsync(
        LogisticsPartnerQueryParameters query, CancellationToken cancellationToken);

    Task<LogisticsPartnerProfileDto> UpsertAsync(
        Guid targetUserId, Guid currentUserId, bool isAdmin,
        UpsertLogisticsPartnerProfileRequest request, CancellationToken cancellationToken);

    Task<LogisticsPartnerProfileDto> VerifyAsync(
        Guid targetUserId, Guid verifierUserId, VerifyLogisticsPartnerRequest request,
        CancellationToken cancellationToken);

    Task<LogisticsPartnerProfileDto> UpsertServiceAreaAsync(
        Guid targetUserId, Guid currentUserId, bool isAdmin,
        UpsertLogisticsServiceAreaRequest request, CancellationToken cancellationToken);

    Task<LogisticsPartnerProfileDto> RemoveServiceAreaAsync(
        Guid targetUserId, Guid currentUserId, bool isAdmin, Guid serviceAreaId,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid targetUserId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}

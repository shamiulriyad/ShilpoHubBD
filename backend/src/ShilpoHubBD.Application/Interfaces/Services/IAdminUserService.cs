using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAdminUserService
{
    Task<AdminUserDetailDto> CreateLogisticsUserAsync(Guid profileId,
        CreateGovernmentNgoUserRequest request, Guid actorUserId, string? ipAddress, CancellationToken cancellationToken);
    Task<PagedResult<AdminUserListItemDto>> GetPagedAsync(
        AdminUserQueryParameters query, CancellationToken cancellationToken);

    Task<AdminUserDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AdminUserDetailDto> SetActiveAsync(
        Guid id, bool isActive, Guid actorUserId, string? ipAddress, CancellationToken cancellationToken);

    /// <summary>SuperAdmin-only: creates a Government/NGO account directly (self-registration for this role
    /// is disabled). The admin sets the initial password; the user can change it after logging in.</summary>
    Task<AdminUserDetailDto> CreateGovernmentNgoUserAsync(
        CreateGovernmentNgoUserRequest request, Guid actorUserId, string? ipAddress, CancellationToken cancellationToken);
}

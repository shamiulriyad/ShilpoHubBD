using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserListItemDto>> GetPagedAsync(
        AdminUserQueryParameters query, CancellationToken cancellationToken);

    Task<AdminUserDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AdminUserDetailDto> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
}

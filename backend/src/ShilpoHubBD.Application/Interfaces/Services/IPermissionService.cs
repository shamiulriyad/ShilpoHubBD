using ShilpoHubBD.Application.DTOs.Admin;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<PermissionDto> CreateAsync(CreatePermissionRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<List<RoleAdminDto>> GetRolesAsync(CancellationToken cancellationToken);

    Task<RolePermissionsDto> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken);

    Task<RolePermissionsDto> SyncRolePermissionsAsync(
        Guid roleId, Guid actorUserId, SyncRolePermissionsRequest request, CancellationToken cancellationToken);
}

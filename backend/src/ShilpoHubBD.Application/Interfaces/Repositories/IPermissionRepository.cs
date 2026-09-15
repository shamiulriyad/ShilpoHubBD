using ShilpoHubBD.Domain.Entities.Admin;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken);

    Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<List<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken);

    Task AddAsync(Permission permission, CancellationToken cancellationToken);

    void Remove(Permission permission);

    Task<List<Role>> GetAllRolesWithCountsAsync(CancellationToken cancellationToken);

    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken);

    Task<List<RolePermission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken);

    void RemoveRolePermissions(IEnumerable<RolePermission> rolePermissions);

    Task AddRolePermissionsAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken);

    Task<int> CountUsersInRoleAsync(Guid roleId, CancellationToken cancellationToken);

    Task<int> CountPermissionsForRoleAsync(Guid roleId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

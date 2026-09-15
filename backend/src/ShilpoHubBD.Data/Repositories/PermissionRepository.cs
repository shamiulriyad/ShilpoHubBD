using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Admin;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ShilpoHubDbContext _context;

    public PermissionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken)
        => _context.Permissions.OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync(cancellationToken);

    public Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Permissions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => _context.Permissions.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

    public Task<List<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken)
        => _context.Permissions.Where(p => codes.Contains(p.Code)).ToListAsync(cancellationToken);

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken)
        => await _context.Permissions.AddAsync(permission, cancellationToken);

    public void Remove(Permission permission) => _context.Permissions.Remove(permission);

    public Task<List<Role>> GetAllRolesWithCountsAsync(CancellationToken cancellationToken)
        => _context.Roles.OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
        => _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

    public Task<List<RolePermission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken)
        => _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

    public void RemoveRolePermissions(IEnumerable<RolePermission> rolePermissions)
        => _context.RolePermissions.RemoveRange(rolePermissions);

    public async Task AddRolePermissionsAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken)
        => await _context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);

    public Task<int> CountUsersInRoleAsync(Guid roleId, CancellationToken cancellationToken)
        => _context.UserRoles.CountAsync(ur => ur.RoleId == roleId, cancellationToken);

    public Task<int> CountPermissionsForRoleAsync(Guid roleId, CancellationToken cancellationToken)
        => _context.RolePermissions.CountAsync(rp => rp.RoleId == roleId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

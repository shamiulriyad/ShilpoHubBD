using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Admin;

namespace ShilpoHubBD.Application.Services.Admin;

/// <summary>Manages the permission catalogue and which permissions each role is granted.</summary>
public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;

    public PermissionService(IPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PermissionDto>> GetAllAsync(CancellationToken cancellationToken)
        => (await _repository.GetAllAsync(cancellationToken)).Select(p => p.ToDto()).ToList();

    public async Task<PermissionDto> CreateAsync(CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToLowerInvariant();
        if (await _repository.GetByCodeAsync(code, cancellationToken) is not null)
        {
            throw new ConflictException("A permission with this code already exists.");
        }

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            Module = request.Module.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _repository.AddAsync(permission, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return permission.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var permission = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Permission not found.");

        _repository.Remove(permission);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<RoleAdminDto>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _repository.GetAllRolesWithCountsAsync(cancellationToken);
        var result = new List<RoleAdminDto>();
        foreach (var role in roles)
        {
            result.Add(new RoleAdminDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                UserCount = await _repository.CountUsersInRoleAsync(role.Id, cancellationToken),
                PermissionCount = await _repository.CountPermissionsForRoleAsync(role.Id, cancellationToken),
            });
        }

        return result;
    }

    public async Task<RolePermissionsDto> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await _repository.GetRoleByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("Role not found.");
        var rolePermissions = await _repository.GetRolePermissionsAsync(roleId, cancellationToken);

        return new RolePermissionsDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            PermissionCodes = rolePermissions.Select(rp => rp.Permission.Code).ToList(),
        };
    }

    public async Task<RolePermissionsDto> SyncRolePermissionsAsync(
        Guid roleId, Guid actorUserId, SyncRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var role = await _repository.GetRoleByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("Role not found.");

        var requestedCodes = request.PermissionCodes.Distinct().ToList();
        var matchedPermissions = await _repository.GetByCodesAsync(requestedCodes, cancellationToken);
        var unknownCodes = requestedCodes.Except(matchedPermissions.Select(p => p.Code)).ToList();
        if (unknownCodes.Count > 0)
        {
            throw new ConflictException($"Unknown permission code(s): {string.Join(", ", unknownCodes)}.");
        }

        var existing = await _repository.GetRolePermissionsAsync(roleId, cancellationToken);
        _repository.RemoveRolePermissions(existing);

        var now = DateTime.UtcNow;
        var toAdd = matchedPermissions.Select(p => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = p.Id,
            GrantedAt = now,
            GrantedByUserId = actorUserId,
        });
        await _repository.AddRolePermissionsAsync(toAdd, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return new RolePermissionsDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            PermissionCodes = matchedPermissions.Select(p => p.Code).ToList(),
        };
    }
}

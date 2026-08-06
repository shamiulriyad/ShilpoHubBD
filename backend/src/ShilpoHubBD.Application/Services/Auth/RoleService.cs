using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Services.Auth;

public class RoleService : IRoleService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public RoleService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task AssignRoleAsync(Guid userId, string role, Guid assignedByUserId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var roleEntity = await _roleRepository.GetByNameAsync(role, cancellationToken);
        if (roleEntity is null)
        {
            throw new NotFoundException("Role not found.");
        }

        if (user.UserRoles.Any(ur => ur.RoleId == roleEntity.Id))
        {
            throw new ConflictException("User already has this role.");
        }

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = roleEntity.Id,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = assignedByUserId,
        });

        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var userRole = user.UserRoles.FirstOrDefault(ur => ur.Role.Name.Equals(role, StringComparison.OrdinalIgnoreCase));
        if (userRole is null)
        {
            throw new NotFoundException("User does not have this role.");
        }

        if (user.UserRoles.Count == 1)
        {
            throw new ConflictException("Cannot remove a user's only remaining role.");
        }

        user.UserRoles.Remove(userRole);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IRoleService
{
    Task AssignRoleAsync(Guid userId, string role, Guid assignedByUserId, CancellationToken cancellationToken);
    Task RemoveRoleAsync(Guid userId, string role, CancellationToken cancellationToken);
}

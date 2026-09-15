using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAdminUserRepository
{
    Task<(List<User> Items, int TotalCount)> GetPagedAsync(
        AdminUserQueryParameters query, CancellationToken cancellationToken);

    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

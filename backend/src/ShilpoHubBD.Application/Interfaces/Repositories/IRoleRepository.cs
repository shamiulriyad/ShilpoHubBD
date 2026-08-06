using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<List<Role>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken);
}

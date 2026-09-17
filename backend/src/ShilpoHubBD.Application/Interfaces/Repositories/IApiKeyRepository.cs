using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IApiKeyRepository
{
    Task AddAsync(ApiKey key, CancellationToken cancellationToken);
    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<ApiKey> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using ShilpoHubBD.Domain.Entities.Traceability;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ITraceabilityRepository
{
    Task<ProductTraceability?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductTraceability?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task<bool> ExistsByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task AddAsync(ProductTraceability traceability, CancellationToken cancellationToken);
    void Remove(ProductTraceability traceability);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

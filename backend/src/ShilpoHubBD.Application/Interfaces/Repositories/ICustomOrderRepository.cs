using ShilpoHubBD.Domain.Entities.CustomOrders;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICustomOrderRepository
{
    Task<CustomOrderRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<CustomOrderRequest>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken);
    Task<List<CustomOrderRequest>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task AddAsync(CustomOrderRequest request, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IOrderComplaintRepository
{
    Task<OrderComplaint?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<OrderComplaint>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task<List<OrderComplaint>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken);

    // A complaint the customer filed that is not yet closed as Satisfied or Withdrawn: it blocks rating that product.
    Task<bool> HasUnsettledAsync(Guid customerId, Guid productId, CancellationToken cancellationToken);
    Task<bool> HasOpenForOrderProductAsync(Guid orderId, Guid productId, CancellationToken cancellationToken);
    Task AddAsync(OrderComplaint complaint, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

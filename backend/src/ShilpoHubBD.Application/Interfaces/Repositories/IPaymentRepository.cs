using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> HasActivePaymentAsync(Guid orderId, CancellationToken cancellationToken);
    Task AddAsync(Payment payment, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

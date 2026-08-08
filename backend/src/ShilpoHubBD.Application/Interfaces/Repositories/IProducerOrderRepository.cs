using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerOrderRepository
{
    Task<(List<OrderItem> Items, int TotalCount)> GetPagedByProducerAsync(
        Guid producerId, OrderItemProducerStatus? status, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken cancellationToken);

    Task<OrderItem?> GetByIdAsync(Guid orderItemId, CancellationToken cancellationToken);

    /// <summary>Unpaged order items for a producer, used for revenue/analytics/customer aggregation.</summary>
    Task<List<OrderItem>> GetByProducerAsync(
        Guid producerId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken);

    Task<Dictionary<Guid, (string FullName, string Email)>> GetCustomerInfoAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

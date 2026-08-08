using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<(List<Order> Items, int TotalCount)> GetPagedByUserAsync(Guid userId, OrderQueryParameters query, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken);
    Task<bool> HasPurchasedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task<bool> HasCompletedOrderFromDistrictAsync(Guid userId, Guid districtId, CancellationToken cancellationToken);
    Task<int> GetCompletedOrderCountAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task AddStatusEventAsync(OrderStatusEvent statusEvent, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

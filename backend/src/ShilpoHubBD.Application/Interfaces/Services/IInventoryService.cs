using ShilpoHubBD.Application.DTOs.Inventory;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IInventoryService
{
    Task<InventoryTransactionDto> AdjustStockAsync(Guid productId, Guid currentUserId, bool isAdmin, AdjustStockRequest request, CancellationToken cancellationToken);
    Task<List<InventoryTransactionDto>> GetHistoryAsync(Guid productId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<List<LowStockProductDto>> GetLowStockAsync(Guid producerId, CancellationToken cancellationToken);
}

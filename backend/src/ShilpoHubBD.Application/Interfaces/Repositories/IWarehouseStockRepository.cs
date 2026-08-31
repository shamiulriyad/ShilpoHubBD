using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IWarehouseStockRepository
{
    // ---- Warehouse context ------------------------------------------
    Task<Warehouse?> GetWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken);

    Task<WarehouseBin?> GetBinAsync(Guid binId, CancellationToken cancellationToken);

    // ---- Stock items ----------------------------------------------
    Task AddStockItemAsync(WarehouseStockItem item, CancellationToken cancellationToken);

    void RemoveStockItem(WarehouseStockItem item);

    Task<WarehouseStockItem?> GetStockItemByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<WarehouseStockItem?> FindMatchingStockItemAsync(
        Guid warehouseId, Guid? binId, string sku, string? batchNumber, CancellationToken cancellationToken);

    Task<(List<WarehouseStockItem> Items, int TotalCount)> GetStockItemsPagedAsync(
        Guid? profileId, WarehouseStockQueryParameters query, CancellationToken cancellationToken);

    // ---- Movements ------------------------------------------------
    Task AddMovementAsync(WarehouseStockMovement movement, CancellationToken cancellationToken);

    Task<(List<WarehouseStockMovement> Items, int TotalCount)> GetMovementsPagedAsync(
        Guid? profileId, WarehouseStockMovementQueryParameters query, CancellationToken cancellationToken);

    // ---- Cross-checks -------------------------------------------
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

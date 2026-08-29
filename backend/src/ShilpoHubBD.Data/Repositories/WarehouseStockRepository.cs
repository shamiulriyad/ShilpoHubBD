using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Repositories;

public class WarehouseStockRepository : IWarehouseStockRepository
{
    private readonly ShilpoHubDbContext _context;

    public WarehouseStockRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<Warehouse?> GetWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken)
        => _context.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId, cancellationToken);

    public Task<WarehouseBin?> GetBinAsync(Guid binId, CancellationToken cancellationToken)
        => _context.WarehouseBins.FirstOrDefaultAsync(b => b.Id == binId, cancellationToken);

    public async Task AddStockItemAsync(WarehouseStockItem item, CancellationToken cancellationToken)
        => await _context.WarehouseStockItems.AddAsync(item, cancellationToken);

    public void RemoveStockItem(WarehouseStockItem item) => _context.WarehouseStockItems.Remove(item);

    public Task<WarehouseStockItem?> GetStockItemByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.WarehouseStockItems
            .Include(i => i.Warehouse)
            .Include(i => i.Bin)
            .Include(i => i.Product)
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<WarehouseStockItem?> FindMatchingStockItemAsync(
        Guid warehouseId, Guid? binId, string sku, string? batchNumber, CancellationToken cancellationToken)
        => _context.WarehouseStockItems.FirstOrDefaultAsync(
            i => i.WarehouseId == warehouseId
                && i.WarehouseBinId == binId
                && i.Sku == sku
                && i.BatchNumber == batchNumber,
            cancellationToken);

    public async Task<(List<WarehouseStockItem> Items, int TotalCount)> GetStockItemsPagedAsync(
        Guid? profileId, WarehouseStockQueryParameters query, CancellationToken cancellationToken)
    {
        var items = _context.WarehouseStockItems
            .Include(i => i.Warehouse)
            .Include(i => i.Bin)
            .Include(i => i.Product)
            .AsQueryable();

        if (profileId.HasValue)
        {
            items = items.Where(i => i.Warehouse.LogisticsPartnerProfileId == profileId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            items = items.Where(i => i.WarehouseId == query.WarehouseId.Value);
        }

        if (query.WarehouseBinId.HasValue)
        {
            items = items.Where(i => i.WarehouseBinId == query.WarehouseBinId.Value);
        }

        if (query.ProductId.HasValue)
        {
            items = items.Where(i => i.ProductId == query.ProductId.Value);
        }

        if (query.OwnerUserId.HasValue)
        {
            items = items.Where(i => i.OwnerUserId == query.OwnerUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<WarehouseStockItemStatus>(query.Status, true, out var status))
        {
            items = items.Where(i => i.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Sku))
        {
            var sku = query.Sku.Trim().ToLower();
            items = items.Where(i => i.Sku.ToLower() == sku);
        }

        if (query.LowStock == true)
        {
            items = items.Where(i => i.QuantityAvailable <= 0);
        }

        if (query.ExpiringSoon == true)
        {
            var horizon = DateTime.UtcNow.AddDays(30);
            items = items.Where(i => i.ExpiryDate != null && i.ExpiryDate <= horizon);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            items = items.Where(i =>
                i.Sku.ToLower().Contains(term)
                || i.Description.ToLower().Contains(term)
                || (i.BatchNumber != null && i.BatchNumber.ToLower().Contains(term)));
        }

        items = items
            .OrderBy(i => i.Sku)
            .ThenByDescending(i => i.UpdatedAt);

        var totalCount = await items.CountAsync(cancellationToken);
        var page = await items
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (page, totalCount);
    }

    public async Task AddMovementAsync(WarehouseStockMovement movement, CancellationToken cancellationToken)
        => await _context.WarehouseStockMovements.AddAsync(movement, cancellationToken);

    public async Task<(List<WarehouseStockMovement> Items, int TotalCount)> GetMovementsPagedAsync(
        Guid? profileId, WarehouseStockMovementQueryParameters query, CancellationToken cancellationToken)
    {
        var movements = _context.WarehouseStockMovements
            .Include(m => m.PerformedBy)
            .AsQueryable();

        if (profileId.HasValue)
        {
            movements = movements.Where(m => m.Warehouse.LogisticsPartnerProfileId == profileId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            movements = movements.Where(m => m.WarehouseId == query.WarehouseId.Value);
        }

        if (query.WarehouseStockItemId.HasValue)
        {
            movements = movements.Where(m => m.WarehouseStockItemId == query.WarehouseStockItemId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Type)
            && Enum.TryParse<WarehouseStockMovementType>(query.Type, true, out var type))
        {
            movements = movements.Where(m => m.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(query.ReferenceType))
        {
            var refType = query.ReferenceType.Trim();
            movements = movements.Where(m => m.ReferenceType == refType);
        }

        if (query.ReferenceId.HasValue)
        {
            movements = movements.Where(m => m.ReferenceId == query.ReferenceId.Value);
        }

        if (query.From.HasValue)
        {
            var from = DateTime.SpecifyKind(query.From.Value, DateTimeKind.Utc);
            movements = movements.Where(m => m.OccurredAt >= from);
        }

        if (query.To.HasValue)
        {
            var to = DateTime.SpecifyKind(query.To.Value, DateTimeKind.Utc);
            movements = movements.Where(m => m.OccurredAt <= to);
        }

        movements = movements.OrderByDescending(m => m.OccurredAt).ThenByDescending(m => m.CreatedAt);

        var totalCount = await movements.CountAsync(cancellationToken);
        var page = await movements
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (page, totalCount);
    }

    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken)
        => _context.Products.AnyAsync(p => p.Id == productId, cancellationToken);

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

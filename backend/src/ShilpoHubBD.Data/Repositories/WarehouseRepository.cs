using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly ShilpoHubDbContext _context;

    public WarehouseRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken)
        => await _context.Warehouses.AddAsync(warehouse, cancellationToken);

    public void Remove(Warehouse warehouse) => _context.Warehouses.Remove(warehouse);

    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Warehouses
            .Include(w => w.Profile)
            .Include(w => w.CreatedBy)
            .Include(w => w.District)
            .Include(w => w.Zones)
            .Include(w => w.Bins).ThenInclude(b => b.Zone)
            .AsSplitQuery()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken)
        => _context.Warehouses.AnyAsync(w => w.Code == code, cancellationToken);

    public async Task<(List<Warehouse> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, WarehouseQueryParameters query, CancellationToken cancellationToken)
    {
        var warehouses = _context.Warehouses
            .Include(w => w.District)
            .Include(w => w.Zones)
            .Include(w => w.Bins)
            .AsQueryable();

        if (profileId.HasValue)
        {
            warehouses = warehouses.Where(w => w.LogisticsPartnerProfileId == profileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Type)
            && Enum.TryParse<WarehouseType>(query.Type, true, out var type))
        {
            warehouses = warehouses.Where(w => w.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<WarehouseStatus>(query.Status, true, out var status))
        {
            warehouses = warehouses.Where(w => w.Status == status);
        }

        if (query.DistrictId.HasValue)
        {
            warehouses = warehouses.Where(w => w.DistrictId == query.DistrictId.Value);
        }

        if (query.HasColdChain.HasValue)
        {
            warehouses = warehouses.Where(w => w.HasColdChain == query.HasColdChain.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            warehouses = warehouses.Where(w =>
                w.Code.ToLower().Contains(term)
                || w.Name.ToLower().Contains(term)
                || w.City.ToLower().Contains(term));
        }

        warehouses = warehouses.OrderByDescending(w => w.CreatedAt);

        var totalCount = await warehouses.CountAsync(cancellationToken);
        var items = await warehouses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Dictionary<Guid, (int Zones, int Bins, int StockItems)>> GetCountsAsync(
        IEnumerable<Guid> warehouseIds, CancellationToken cancellationToken)
    {
        var ids = warehouseIds.Distinct().ToList();

        var zoneCounts = await _context.WarehouseZones
            .Where(z => ids.Contains(z.WarehouseId))
            .GroupBy(z => z.WarehouseId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var binCounts = await _context.WarehouseBins
            .Where(b => ids.Contains(b.WarehouseId))
            .GroupBy(b => b.WarehouseId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var stockCounts = await _context.WarehouseStockItems
            .Where(i => ids.Contains(i.WarehouseId))
            .GroupBy(i => i.WarehouseId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        return ids.ToDictionary(
            id => id,
            id => (
                zoneCounts.GetValueOrDefault(id),
                binCounts.GetValueOrDefault(id),
                stockCounts.GetValueOrDefault(id)));
    }

    public Task<bool> HasStockItemsAsync(Guid warehouseId, CancellationToken cancellationToken)
        => _context.WarehouseStockItems.AnyAsync(i => i.WarehouseId == warehouseId, cancellationToken);

    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

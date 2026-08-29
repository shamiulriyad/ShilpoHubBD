using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IWarehouseRepository
{
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken);

    void Remove(Warehouse warehouse);

    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

    Task<(List<Warehouse> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, WarehouseQueryParameters query, CancellationToken cancellationToken);

    Task<Dictionary<Guid, (int Zones, int Bins, int StockItems)>> GetCountsAsync(
        IEnumerable<Guid> warehouseIds, CancellationToken cancellationToken);

    Task<bool> HasStockItemsAsync(Guid warehouseId, CancellationToken cancellationToken);

    Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

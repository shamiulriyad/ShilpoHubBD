using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IWarehouseService
{
    Task<WarehouseDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreateWarehouseRequest request, CancellationToken cancellationToken);

    Task<PagedResult<WarehouseListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, WarehouseQueryParameters query, CancellationToken cancellationToken);

    Task<WarehouseDto> GetByIdAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task<WarehouseDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateWarehouseRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    // ---- Zones -----------------------------------------------------
    Task<WarehouseDto> AddZoneAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpsertWarehouseZoneRequest request, CancellationToken cancellationToken);

    Task<WarehouseDto> UpdateZoneAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid zoneId, UpsertWarehouseZoneRequest request, CancellationToken cancellationToken);

    Task<WarehouseDto> RemoveZoneAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid zoneId, CancellationToken cancellationToken);

    // ---- Bins ------------------------------------------------------
    Task<WarehouseDto> AddBinAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpsertWarehouseBinRequest request, CancellationToken cancellationToken);

    Task<WarehouseDto> UpdateBinAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid binId, UpsertWarehouseBinRequest request, CancellationToken cancellationToken);

    Task<WarehouseDto> RemoveBinAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid binId, CancellationToken cancellationToken);
}

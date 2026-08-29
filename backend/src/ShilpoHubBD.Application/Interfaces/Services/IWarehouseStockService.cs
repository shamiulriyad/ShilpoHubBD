using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IWarehouseStockService
{
    Task<WarehouseStockItemDto> ReceiveAsync(
        Guid currentUserId, bool isAdmin, ReceiveStockRequest request, CancellationToken cancellationToken);

    Task<PagedResult<WarehouseStockItemListItemDto>> GetStockItemsAsync(
        Guid currentUserId, bool isAdmin, WarehouseStockQueryParameters query, CancellationToken cancellationToken);

    Task<WarehouseStockItemDto> GetStockItemByIdAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task<WarehouseStockItemDto> IssueAsync(
        Guid currentUserId, bool isAdmin, Guid id, IssueStockRequest request, CancellationToken cancellationToken);

    Task<WarehouseStockItemDto> TransferAsync(
        Guid currentUserId, bool isAdmin, Guid id, TransferStockRequest request, CancellationToken cancellationToken);

    Task<WarehouseStockItemDto> AdjustAsync(
        Guid currentUserId, bool isAdmin, Guid id, AdjustStockRequest request, CancellationToken cancellationToken);

    Task<WarehouseStockItemDto> ReserveAsync(
        Guid currentUserId, bool isAdmin, Guid id, ReserveStockRequest request, CancellationToken cancellationToken);

    Task<WarehouseStockItemDto> ReleaseReservationAsync(
        Guid currentUserId, bool isAdmin, Guid id, ReserveStockRequest request, CancellationToken cancellationToken);

    Task DeleteStockItemAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task<PagedResult<WarehouseStockMovementDto>> GetMovementsAsync(
        Guid currentUserId, bool isAdmin, WarehouseStockMovementQueryParameters query, CancellationToken cancellationToken);
}

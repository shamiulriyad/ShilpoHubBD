using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IOrderService
{
    Task<PagedResult<OrderListItemDto>> GetMyOrdersAsync(Guid userId, OrderQueryParameters query, CancellationToken cancellationToken);
    Task<OrderDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<OrderTrackingDto> GetTrackingAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken);
    Task<OrderDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancelOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDto> RequestReturnAsync(Guid id, Guid currentUserId, bool isAdmin, ReturnOrderRequest request, CancellationToken cancellationToken);

    Task<OrderDto> ConfirmAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderDto> ShipAsync(Guid id, ShipOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDto> DeliverAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderDto> ApproveReturnAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderDto> RejectReturnAsync(Guid id, RejectReturnRequest request, CancellationToken cancellationToken);
    Task<OrderDto> RefundAsync(Guid id, RefundOrderRequest request, CancellationToken cancellationToken);
}

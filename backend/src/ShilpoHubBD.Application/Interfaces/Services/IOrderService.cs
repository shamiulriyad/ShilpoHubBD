using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IOrderService
{
    // Brings the customer-facing Order.Status in line with what the producers have done to its items.
    Task SyncStatusFromFulfillmentAsync(Guid orderId, CancellationToken cancellationToken);
    // A logistics partner delivered a shipment: the items handed over under that tracking number become
    // Delivered and the order status rolls up.
    Task HandleShipmentDeliveredAsync(Guid orderId, string trackingNumber, CancellationToken cancellationToken);

    Task<PagedResult<OrderListItemDto>> GetMyOrdersAsync(Guid userId, OrderQueryParameters query, CancellationToken cancellationToken);
    Task<OrderDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<OrderTrackingDto> GetTrackingAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken);
    Task<OrderDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancelOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDto> RequestReturnAsync(Guid id, Guid currentUserId, bool isAdmin, ReturnOrderRequest request, CancellationToken cancellationToken);

    // The returned goods reached the producer: stock is restored and any payment the customer already
    // made is refunded. Returns the refunded amount (0 when nothing had been paid, e.g. cash on delivery).
    Task<decimal> CompleteReturnAsync(Guid orderId, CancellationToken cancellationToken);
    Task RecordStatusNoteAsync(Guid orderId, string note, CancellationToken cancellationToken);

    Task<OrderDto> ConfirmAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderDto> ShipAsync(Guid id, ShipOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDto> DeliverAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderDto> ApproveReturnAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderDto> RejectReturnAsync(Guid id, RejectReturnRequest request, CancellationToken cancellationToken);
    Task<OrderDto> RefundAsync(Guid id, RefundOrderRequest request, CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Services.Commerce;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentService _paymentService;

    public OrderService(
        IOrderRepository orderRepository, ICartRepository cartRepository, IDistrictRepository districtRepository,
        IPaymentRepository paymentRepository, IPaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _districtRepository = districtRepository;
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
    }

    public async Task<PagedResult<OrderListItemDto>> GetMyOrdersAsync(Guid userId, OrderQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _orderRepository.GetPagedByUserAsync(userId, query, cancellationToken);
        return new PagedResult<OrderListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureOwnershipOrAdmin(order, currentUserId, isAdmin);
        return ToDto(order);
    }

    public async Task<OrderTrackingDto> GetTrackingAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureOwnershipOrAdmin(order, currentUserId, isAdmin);

        return new OrderTrackingDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            TrackingNumber = order.TrackingNumber,
            Carrier = order.Carrier,
            Events = order.StatusHistory
                .OrderBy(e => e.CreatedAt)
                .Select(e => new OrderStatusEventDto { Status = e.Status.ToString(), Note = e.Note, CreatedAt = e.CreatedAt })
                .ToList(),
        };
    }

    public async Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken)
    {
        var district = await _districtRepository.GetByIdAsync(request.ShippingDistrictId, cancellationToken)
            ?? throw new NotFoundException("District not found.");

        var cartItems = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cartItems.Count == 0)
        {
            throw new ConflictException("Your cart is empty.");
        }

        foreach (var cartItem in cartItems)
        {
            if (!cartItem.Product.IsActive)
            {
                throw new ConflictException($"'{cartItem.Product.Name}' is no longer available.");
            }

            var availableStock = cartItem.ProductVariant?.Stock ?? cartItem.Product.Stock;
            if (cartItem.Quantity > availableStock)
            {
                throw new ConflictException($"Only {availableStock} unit(s) of '{cartItem.Product.Name}' are available.");
            }
        }

        var now = DateTime.UtcNow;
        var subtotal = cartItems.Sum(c => (c.ProductVariant?.Price ?? c.Product.Price) * c.Quantity);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = await GenerateUniqueOrderNumberAsync(cancellationToken),
            UserId = userId,
            Status = OrderStatus.Pending,
            PaymentMethod = request.PaymentMethod,
            Subtotal = subtotal,
            Total = subtotal,
            RecipientName = request.RecipientName.Trim(),
            RecipientPhone = request.RecipientPhone.Trim(),
            ShippingAddressLine = request.ShippingAddressLine.Trim(),
            ShippingDistrictId = request.ShippingDistrictId,
            ShippingDistrict = district,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var cartItem in cartItems)
        {
            var unitPrice = cartItem.ProductVariant?.Price ?? cartItem.Product.Price;

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Product = cartItem.Product,
                ProductName = cartItem.Product.Name,
                ProductImageUrl = cartItem.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
                ProductVariantId = cartItem.ProductVariantId,
                ProductVariant = cartItem.ProductVariant,
                VariantName = cartItem.ProductVariant?.Name,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = unitPrice * cartItem.Quantity,
            });

            if (cartItem.ProductVariant is not null)
            {
                cartItem.ProductVariant.Stock -= cartItem.Quantity;
            }
            else
            {
                cartItem.Product.Stock -= cartItem.Quantity;
            }

            cartItem.Product.SalesCount += cartItem.Quantity;
        }

        AddStatusEvent(order, OrderStatus.Pending, "Order placed.");

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        await _cartRepository.ClearAsync(userId, cancellationToken);

        return ToDto(order);
    }

    public async Task<OrderDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureOwnershipOrAdmin(order, currentUserId, isAdmin);

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
        {
            throw new ConflictException("Only pending or processing orders can be cancelled.");
        }

        RestoreStock(order);
        CancelProducerFulfillment(order);

        order.Status = OrderStatus.Cancelled;
        order.CancelReason = request.Reason?.Trim();
        order.UpdatedAt = DateTime.UtcNow;

        // A cancelled order that was already paid is refunded in full. The refund shares the scoped
        // DbContext and saves the cancellation and stock restore in the same commit -- if the payment
        // provider refuses the refund it throws before anything is saved and the order stays as it was.
        var refunded = await RefundPaidPaymentsAsync(order, cancellationToken);
        var note = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        if (refunded > 0)
        {
            order.RefundAmount = refunded;
            order.RefundReason = "Order cancelled by customer";
            note = $"{(note is null ? "Cancelled" : note)} -- refund of ৳{refunded:0.##} issued.";
        }

        await RecordStatusEventAsync(order, OrderStatus.Cancelled, note, cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    // Producers work on OrderItems (Accepted -> Processing -> Shipped -> Delivered), but the customer
    // sees Order.Status, which only the admin endpoints used to move -- so a producer could ship an
    // order and the customer still saw "Pending". After each producer action the order is rolled up:
    //   any item accepted/processing/shipped/delivered ........ Processing
    //   every live item shipped or delivered .................. Shipped
    //   every live item delivered ............................. Delivered
    //   every item declined/cancelled ......................... Cancelled (stock restored, payment refunded)
    // ("live" = not rejected/cancelled). It only ever moves an order forward.
    public async Task SyncStatusFromFulfillmentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null || order.Status is not (OrderStatus.Pending or OrderStatus.Processing or OrderStatus.Shipped))
        {
            return;
        }

        var live = order.Items
            .Where(i => i.ProducerStatus is not (OrderItemProducerStatus.Rejected or OrderItemProducerStatus.Cancelled))
            .ToList();

        if (live.Count == 0)
        {
            await CancelAsync(order.Id, order.UserId, true, new CancelOrderRequest { Reason = "Every item was declined by the producer." }, cancellationToken);
            return;
        }

        var target = order.Status;
        if (live.All(i => i.ProducerStatus == OrderItemProducerStatus.Delivered))
        {
            target = OrderStatus.Delivered;
        }
        else if (live.All(i => i.ProducerStatus is OrderItemProducerStatus.Shipped or OrderItemProducerStatus.Delivered))
        {
            target = OrderStatus.Shipped;
        }
        else if (live.Any(i => i.ProducerStatus is OrderItemProducerStatus.Accepted or OrderItemProducerStatus.Processing
            or OrderItemProducerStatus.Shipped or OrderItemProducerStatus.Delivered))
        {
            target = OrderStatus.Processing;
        }

        static int Rank(OrderStatus s) => s switch { OrderStatus.Pending => 0, OrderStatus.Processing => 1, OrderStatus.Shipped => 2, _ => 3 };
        if (Rank(target) <= Rank(order.Status))
        {
            return;
        }

        string note;
        switch (target)
        {
            case OrderStatus.Shipped:
                var shipped = live.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.TrackingNumber));
                order.TrackingNumber ??= shipped?.TrackingNumber;
                order.Carrier ??= shipped?.Carrier;
                note = shipped is null
                    ? "Your order has been shipped."
                    : $"Shipped via {shipped.Carrier}, tracking number {shipped.TrackingNumber}.";
                break;
            case OrderStatus.Delivered:
                note = "Order delivered.";
                break;
            default:
                note = "The producer accepted your order and is preparing it.";
                break;
        }

        order.Status = target;
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, target, note, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleShipmentDeliveredAsync(Guid orderId, string trackingNumber, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var changed = false;
        foreach (var item in order.Items.Where(i => i.ProducerStatus == OrderItemProducerStatus.Shipped
            && string.Equals(i.TrackingNumber, trackingNumber, StringComparison.OrdinalIgnoreCase)))
        {
            item.ProducerStatus = OrderItemProducerStatus.Delivered;
            item.DeliveredAt = now;
            changed = true;
        }

        if (changed)
        {
            await _orderRepository.SaveChangesAsync(cancellationToken);
            await SyncStatusFromFulfillmentAsync(orderId, cancellationToken);
        }
    }

    private async Task<decimal> RefundPaidPaymentsAsync(Order order, CancellationToken cancellationToken)
    {
        decimal total = 0;
        var payments = await _paymentRepository.GetByOrderIdAsync(order.Id, cancellationToken);
        foreach (var payment in payments.Where(p => p.Status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded))
        {
            var remaining = payment.Amount - payment.RefundedAmount;
            if (remaining <= 0)
            {
                continue;
            }

            await _paymentService.RefundAsync(payment.Id, new DTOs.Commerce.RefundPaymentRequest { Amount = remaining, Reason = "Order cancelled" }, cancellationToken);
            total += remaining;
        }

        return total;
    }

    public async Task<OrderDto> RequestReturnAsync(Guid id, Guid currentUserId, bool isAdmin, ReturnOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureOwnershipOrAdmin(order, currentUserId, isAdmin);
        EnsureStatus(order, OrderStatus.Delivered);

        order.Status = OrderStatus.ReturnRequested;
        order.ReturnReason = request.Reason.Trim();
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, OrderStatus.ReturnRequested, request.Reason, cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> ConfirmAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureStatus(order, OrderStatus.Pending);

        order.Status = OrderStatus.Processing;
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, OrderStatus.Processing, "Order confirmed and is being processed.", cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> ShipAsync(Guid id, ShipOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureStatus(order, OrderStatus.Processing);

        order.Status = OrderStatus.Shipped;
        order.TrackingNumber = request.TrackingNumber.Trim();
        order.Carrier = request.Carrier.Trim();
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, OrderStatus.Shipped, $"Shipped via {order.Carrier}, tracking number {order.TrackingNumber}.", cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> DeliverAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureStatus(order, OrderStatus.Shipped);

        order.Status = OrderStatus.Delivered;
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, OrderStatus.Delivered, "Order delivered.", cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> ApproveReturnAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureStatus(order, OrderStatus.ReturnRequested);

        RestoreStock(order);

        order.Status = OrderStatus.Returned;
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, OrderStatus.Returned, "Return approved.", cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> RejectReturnAsync(Guid id, RejectReturnRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureStatus(order, OrderStatus.ReturnRequested);

        order.Status = OrderStatus.Delivered;
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, OrderStatus.Delivered, request.Note ?? "Return request rejected.", cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> RefundAsync(Guid id, RefundOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        EnsureStatus(order, OrderStatus.Returned);

        var amount = request.Amount ?? order.Total;
        if (amount > order.Total)
        {
            throw new ConflictException("Refund amount cannot exceed the order total.");
        }

        order.Status = OrderStatus.Refunded;
        order.RefundAmount = amount;
        order.RefundReason = request.Reason?.Trim();
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, OrderStatus.Refunded, $"Refunded ৳{amount}.", cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    private async Task<string> GenerateUniqueOrderNumberAsync(CancellationToken cancellationToken)
    {
        string orderNumber;
        do
        {
            orderNumber = $"SH-ORD-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _orderRepository.ExistsByOrderNumberAsync(orderNumber, cancellationToken));

        return orderNumber;
    }

    private static void RestoreStock(Order order)
    {
        foreach (var item in order.Items)
        {
            if (item.ProductVariant is not null)
            {
                item.ProductVariant.Stock += item.Quantity;
            }
            else
            {
                item.Product.Stock += item.Quantity;
            }

            item.Product.SalesCount = Math.Max(0, item.Product.SalesCount - item.Quantity);
        }
    }

    // Keeps per-producer fulfillment status (see OrderItem.ProducerStatus) in sync when the
    // customer or admin cancels the whole order before any producer has shipped their part of it.
    private static void CancelProducerFulfillment(Order order)
    {
        foreach (var item in order.Items)
        {
            if (item.ProducerStatus is OrderItemProducerStatus.Pending or OrderItemProducerStatus.Accepted or OrderItemProducerStatus.Processing)
            {
                item.ProducerStatus = OrderItemProducerStatus.Cancelled;
            }
        }
    }

    // Only safe for a brand-new, not-yet-tracked Order (see CheckoutAsync): appending to a
    // collection navigation on an already-tracked entity gets mis-detected as an UPDATE by EF Core's
    // change tracker instead of an INSERT. For any already-tracked order use RecordStatusEventAsync.
    private static void AddStatusEvent(Order order, OrderStatus status, string? note)
    {
        order.StatusHistory.Add(new OrderStatusEvent
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = status,
            Note = note,
            CreatedAt = DateTime.UtcNow,
        });
    }

    private async Task RecordStatusEventAsync(Order order, OrderStatus status, string? note, CancellationToken cancellationToken)
    {
        await _orderRepository.AddStatusEventAsync(new OrderStatusEvent
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = status,
            Note = note,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);
    }

    private static void EnsureStatus(Order order, OrderStatus expected)
    {
        if (order.Status != expected)
        {
            throw new ConflictException($"Order must be in '{expected}' status for this action (current: '{order.Status}').");
        }
    }

    private static void EnsureOwnershipOrAdmin(Order order, Guid currentUserId, bool isAdmin)
    {
        if (!isAdmin && order.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this order.");
        }
    }

    private static OrderDto ToDto(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        Status = order.Status.ToString(),
        PaymentMethod = order.PaymentMethod.ToString(),
        Subtotal = order.Subtotal,
        Total = order.Total,
        RecipientName = order.RecipientName,
        RecipientPhone = order.RecipientPhone,
        ShippingAddressLine = order.ShippingAddressLine,
        ShippingDistrictId = order.ShippingDistrictId,
        ShippingDistrictName = order.ShippingDistrict.Name,
        TrackingNumber = order.TrackingNumber,
        Carrier = order.Carrier,
        CancelReason = order.CancelReason,
        ReturnReason = order.ReturnReason,
        RefundAmount = order.RefundAmount,
        RefundReason = order.RefundReason,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ProductImageUrl = i.ProductImageUrl,
            ProductVariantId = i.ProductVariantId,
            VariantName = i.VariantName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal,
        }).ToList(),
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
    };

    private static OrderListItemDto ToListItemDto(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        Status = order.Status.ToString(),
        Total = order.Total,
        ItemCount = order.Items.Count,
        CreatedAt = order.CreatedAt,
    };
}

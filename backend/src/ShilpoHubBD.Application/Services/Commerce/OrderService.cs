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

    public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, IDistrictRepository districtRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _districtRepository = districtRepository;
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

        order.Status = OrderStatus.Cancelled;
        order.CancelReason = request.Reason?.Trim();
        order.UpdatedAt = DateTime.UtcNow;
        await RecordStatusEventAsync(order, OrderStatus.Cancelled, request.Reason, cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
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

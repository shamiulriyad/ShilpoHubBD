using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Commerce;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }

    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;

    public Guid ShippingDistrictId { get; set; }
    public District ShippingDistrict { get; set; } = null!;

    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }

    public string? CancelReason { get; set; }
    public string? ReturnReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusEvent> StatusHistory { get; set; } = new List<OrderStatusEvent>();
}

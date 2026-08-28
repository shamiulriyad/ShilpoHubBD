using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Commerce;

public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public string? VariantName { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    // Per-producer fulfillment tracking, independent of the order-wide Status/tracking (which
    // stays admin/logistics-driven) since a single order can contain items from multiple producers.
    public OrderItemProducerStatus ProducerStatus { get; set; } = OrderItemProducerStatus.Pending;
    public string? ProducerNote { get; set; }
    public DateTime? ProducerRespondedAt { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

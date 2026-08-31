using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>One line of goods on a <see cref="ReturnRequest"/>.</summary>
public class ReturnItem
{
    public Guid Id { get; set; }

    public Guid ReturnRequestId { get; set; }
    public ReturnRequest ReturnRequest { get; set; } = null!;

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public string? Sku { get; set; }
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public int QuantityReceived { get; set; }
    public int RestockedQuantity { get; set; }

    public ReturnItemCondition Condition { get; set; } = ReturnItemCondition.NotReceived;
    public ReturnDisposition Disposition { get; set; } = ReturnDisposition.Pending;

    public decimal? UnitRefundAmount { get; set; }

    public string? Notes { get; set; }
}

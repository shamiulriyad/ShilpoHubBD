using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Procurement;

public class ProcurementItem
{
    public Guid Id { get; set; }

    public Guid ProcurementRequestId { get; set; }
    public ProcurementRequest ProcurementRequest { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Specifications { get; set; }
}

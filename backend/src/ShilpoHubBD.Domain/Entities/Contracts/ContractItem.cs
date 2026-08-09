using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Contracts;

public class ContractItem
{
    public Guid Id { get; set; }

    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Specifications { get; set; }
}

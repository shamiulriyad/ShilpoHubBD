namespace ShilpoHubBD.Application.DTOs.Contracts;

public class ContractItemInput
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Specifications { get; set; }
}

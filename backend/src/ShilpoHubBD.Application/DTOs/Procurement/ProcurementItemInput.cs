namespace ShilpoHubBD.Application.DTOs.Procurement;

public class ProcurementItemInput
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Specifications { get; set; }
}

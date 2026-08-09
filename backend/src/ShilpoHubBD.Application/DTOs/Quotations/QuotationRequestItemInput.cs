namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationRequestItemInput
{
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public int Quantity { get; set; }
    public decimal? TargetPrice { get; set; }
    public string? Specifications { get; set; }
}

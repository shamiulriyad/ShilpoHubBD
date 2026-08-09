namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationRequestItemDto
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public decimal? TargetPrice { get; set; }
    public string? Specifications { get; set; }
}

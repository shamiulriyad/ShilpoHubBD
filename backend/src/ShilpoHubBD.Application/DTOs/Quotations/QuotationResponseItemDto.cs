namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationResponseItemDto
{
    public Guid Id { get; set; }
    public Guid QuotationRequestItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public decimal QuotedUnitPrice { get; set; }
    public int QuotedQuantity { get; set; }
    public string? Notes { get; set; }
}

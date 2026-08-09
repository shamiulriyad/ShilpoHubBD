namespace ShilpoHubBD.Application.DTOs.Quotations;

public class SubmitQuotationResponseItemInput
{
    public Guid QuotationRequestItemId { get; set; }
    public decimal QuotedUnitPrice { get; set; }
    public int QuotedQuantity { get; set; }
    public string? Notes { get; set; }
}

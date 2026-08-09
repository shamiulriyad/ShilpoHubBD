namespace ShilpoHubBD.Domain.Entities.Quotations;

public class QuotationResponseItem
{
    public Guid Id { get; set; }

    public Guid QuotationResponseId { get; set; }
    public QuotationResponse QuotationResponse { get; set; } = null!;

    public Guid QuotationRequestItemId { get; set; }
    public QuotationRequestItem QuotationRequestItem { get; set; } = null!;

    public decimal QuotedUnitPrice { get; set; }
    public int QuotedQuantity { get; set; }
    public string? Notes { get; set; }
}

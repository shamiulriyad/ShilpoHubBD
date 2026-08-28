namespace ShilpoHubBD.Domain.Entities.Quotations;

public class QuotationStatusEvent
{
    public Guid Id { get; set; }

    public Guid QuotationRequestId { get; set; }
    public QuotationRequest QuotationRequest { get; set; } = null!;

    public QuotationRequestStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

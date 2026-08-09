namespace ShilpoHubBD.Domain.Entities.Quotations;

public class QuotationResponse
{
    public Guid Id { get; set; }

    public Guid QuotationRequestProducerId { get; set; }
    public QuotationRequestProducer QuotationRequestProducer { get; set; } = null!;

    public decimal TotalPrice { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Notes { get; set; }

    public QuotationResponseStatus Status { get; set; } = QuotationResponseStatus.Submitted;
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<QuotationResponseItem> Items { get; set; } = new List<QuotationResponseItem>();
}

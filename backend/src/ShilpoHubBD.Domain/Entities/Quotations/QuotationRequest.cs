using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Quotations;

public class QuotationRequest
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public DateTime RequiredDeliveryDate { get; set; }

    public QuotationRequestStatus Status { get; set; } = QuotationRequestStatus.Sent;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<QuotationRequestItem> Items { get; set; } = new List<QuotationRequestItem>();
    public ICollection<QuotationRequestProducer> Recipients { get; set; } = new List<QuotationRequestProducer>();
    public ICollection<QuotationStatusEvent> StatusHistory { get; set; } = new List<QuotationStatusEvent>();
}

using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Quotations;

public class QuotationRequestProducer
{
    public Guid Id { get; set; }

    public Guid QuotationRequestId { get; set; }
    public QuotationRequest QuotationRequest { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public QuotationRecipientStatus Status { get; set; } = QuotationRecipientStatus.Invited;
    public DateTime InvitedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    public QuotationResponse? Response { get; set; }
}

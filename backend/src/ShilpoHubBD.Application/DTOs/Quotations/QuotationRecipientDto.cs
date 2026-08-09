using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationRecipientDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public QuotationRecipientStatus Status { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public QuotationResponseDto? Response { get; set; }
}

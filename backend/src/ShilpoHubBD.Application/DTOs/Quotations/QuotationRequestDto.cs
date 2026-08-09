using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public DateTime RequiredDeliveryDate { get; set; }
    public QuotationRequestStatus Status { get; set; }

    public List<QuotationRequestItemDto> Items { get; set; } = new();
    public List<QuotationRecipientDto> Recipients { get; set; } = new();
    public List<QuotationStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

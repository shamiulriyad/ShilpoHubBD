using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationRequestListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime RequiredDeliveryDate { get; set; }
    public QuotationRequestStatus Status { get; set; }
    public int ItemCount { get; set; }
    public int RecipientCount { get; set; }
    public int ResponseCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

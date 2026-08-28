using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationStatusEventDto
{
    public QuotationRequestStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

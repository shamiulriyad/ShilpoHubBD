using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.DTOs.Quotations;

public class QuotationResponseDecisionRequest
{
    public QuotationResponseStatus Status { get; set; }
    public string? DecisionNotes { get; set; }
}

using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Application.DTOs.Investment;

public class InvestmentProposalListItemDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public string BusinessPartnerName { get; set; } = string.Empty;
    public decimal InvestmentAmount { get; set; }
    public InvestmentProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

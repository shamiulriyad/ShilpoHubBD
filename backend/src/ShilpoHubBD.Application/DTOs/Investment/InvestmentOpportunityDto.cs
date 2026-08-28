using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Application.DTOs.Investment;

public class InvestmentOpportunityDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public decimal FundingRequirement { get; set; }
    public decimal FundingSecured { get; set; }
    public InvestmentOpportunityStatus Status { get; set; }
    public int ProposalCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

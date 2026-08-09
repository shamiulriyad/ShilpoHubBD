using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class OpportunityDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal FundingGoal { get; set; }
    public decimal FundingSecured { get; set; }
    public SponsorshipOpportunityStatus Status { get; set; }
    public int ProposalCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

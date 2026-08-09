using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Investment;

public class InvestmentOpportunity
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public decimal FundingRequirement { get; set; }

    public InvestmentOpportunityStatus Status { get; set; } = InvestmentOpportunityStatus.Open;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<InvestmentProposal> Proposals { get; set; } = new List<InvestmentProposal>();
}

using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.CSRSponsorship;

public class SponsorshipOpportunity
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal FundingGoal { get; set; }

    public SponsorshipOpportunityStatus Status { get; set; } = SponsorshipOpportunityStatus.Open;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SponsorshipProposal> Proposals { get; set; } = new List<SponsorshipProposal>();
}

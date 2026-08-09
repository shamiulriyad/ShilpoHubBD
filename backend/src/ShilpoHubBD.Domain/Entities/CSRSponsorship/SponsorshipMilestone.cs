namespace ShilpoHubBD.Domain.Entities.CSRSponsorship;

public class SponsorshipMilestone
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public SponsorshipProposal Proposal { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public SponsorshipMilestoneStatus Status { get; set; } = SponsorshipMilestoneStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}

namespace ShilpoHubBD.Domain.Entities.CSRSponsorship;

public class SponsorshipStatusEvent
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public SponsorshipProposal Proposal { get; set; } = null!;

    public SponsorshipProposalStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

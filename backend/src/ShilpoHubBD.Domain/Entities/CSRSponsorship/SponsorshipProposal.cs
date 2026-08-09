using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.CSRSponsorship;

public class SponsorshipProposal
{
    public Guid Id { get; set; }

    public Guid OpportunityId { get; set; }
    public SponsorshipOpportunity Opportunity { get; set; } = null!;

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public decimal FundingAmount { get; set; }
    public string? ProposalMessage { get; set; }

    public SponsorshipProposalStatus Status { get; set; } = SponsorshipProposalStatus.Submitted;
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SponsorshipMilestone> Milestones { get; set; } = new List<SponsorshipMilestone>();
    public ICollection<SponsorshipProgressUpdate> ProgressUpdates { get; set; } = new List<SponsorshipProgressUpdate>();
    public ICollection<SponsorshipImpactRecord> ImpactRecords { get; set; } = new List<SponsorshipImpactRecord>();
    public ICollection<SponsorshipStatusEvent> StatusHistory { get; set; } = new List<SponsorshipStatusEvent>();
}

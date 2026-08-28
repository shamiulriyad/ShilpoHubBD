namespace ShilpoHubBD.Domain.Entities.CSRSponsorship;

public class SponsorshipImpactRecord
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public SponsorshipProposal Proposal { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime RecordedAt { get; set; }
}

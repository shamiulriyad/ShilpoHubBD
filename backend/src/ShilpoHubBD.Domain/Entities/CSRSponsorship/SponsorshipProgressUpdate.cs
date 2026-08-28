using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.CSRSponsorship;

public class SponsorshipProgressUpdate
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public SponsorshipProposal Proposal { get; set; } = null!;

    public Guid AuthorUserId { get; set; }
    public User Author { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

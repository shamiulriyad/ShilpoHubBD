using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class ProposalDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;

    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public decimal FundingAmount { get; set; }
    public string? ProposalMessage { get; set; }

    public SponsorshipProposalStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<SponsorshipMilestoneDto> Milestones { get; set; } = new();
    public List<ProgressUpdateDto> ProgressUpdates { get; set; } = new();
    public List<ImpactRecordDto> ImpactRecords { get; set; } = new();
    public List<SponsorshipStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

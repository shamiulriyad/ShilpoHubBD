using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Application.DTOs.Investment;

public class InvestmentProposalDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;

    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public decimal InvestmentAmount { get; set; }
    public string? ProposalMessage { get; set; }

    public InvestmentProposalStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<InvestmentMilestoneDto> Milestones { get; set; } = new();
    public List<InvestmentDocumentDto> Documents { get; set; } = new();
    public List<InvestmentStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

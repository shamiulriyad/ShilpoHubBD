using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Investment;

public class InvestmentProposal
{
    public Guid Id { get; set; }

    public Guid OpportunityId { get; set; }
    public InvestmentOpportunity Opportunity { get; set; } = null!;

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public decimal InvestmentAmount { get; set; }
    public string? ProposalMessage { get; set; }

    public InvestmentProposalStatus Status { get; set; } = InvestmentProposalStatus.Submitted;
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<InvestmentMilestone> Milestones { get; set; } = new List<InvestmentMilestone>();
    public ICollection<InvestmentDocument> Documents { get; set; } = new List<InvestmentDocument>();
    public ICollection<InvestmentStatusEvent> StatusHistory { get; set; } = new List<InvestmentStatusEvent>();
}

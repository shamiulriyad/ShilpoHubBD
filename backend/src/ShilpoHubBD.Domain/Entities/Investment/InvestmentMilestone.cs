namespace ShilpoHubBD.Domain.Entities.Investment;

public class InvestmentMilestone
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public InvestmentProposal Proposal { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public InvestmentMilestoneStatus Status { get; set; } = InvestmentMilestoneStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}

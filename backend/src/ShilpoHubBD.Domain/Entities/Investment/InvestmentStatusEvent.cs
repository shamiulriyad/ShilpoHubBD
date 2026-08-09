namespace ShilpoHubBD.Domain.Entities.Investment;

public class InvestmentStatusEvent
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public InvestmentProposal Proposal { get; set; } = null!;

    public InvestmentProposalStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

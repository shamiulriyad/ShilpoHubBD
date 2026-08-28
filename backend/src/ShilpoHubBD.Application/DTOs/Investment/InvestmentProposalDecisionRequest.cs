namespace ShilpoHubBD.Application.DTOs.Investment;

public class InvestmentProposalDecisionRequest
{
    public bool Approve { get; set; }
    public string? DecisionNotes { get; set; }
}

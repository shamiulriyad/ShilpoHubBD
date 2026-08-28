namespace ShilpoHubBD.Application.DTOs.Investment;

public class SubmitInvestmentProposalRequest
{
    public decimal InvestmentAmount { get; set; }
    public string? ProposalMessage { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.Investment;

public class CreateInvestmentOpportunityRequest
{
    public string Title { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public decimal FundingRequirement { get; set; }
}

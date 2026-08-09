namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class CreateOpportunityRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal FundingGoal { get; set; }
}

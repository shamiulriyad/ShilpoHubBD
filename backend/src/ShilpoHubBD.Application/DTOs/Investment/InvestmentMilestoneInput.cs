namespace ShilpoHubBD.Application.DTOs.Investment;

public class InvestmentMilestoneInput
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class SponsorshipMilestoneInput
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
}

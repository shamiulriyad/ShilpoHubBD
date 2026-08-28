namespace ShilpoHubBD.Application.DTOs.Research;

public class UpdateResearchMilestoneRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public int OrderIndex { get; set; }
}

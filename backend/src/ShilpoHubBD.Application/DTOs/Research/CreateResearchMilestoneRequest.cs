namespace ShilpoHubBD.Application.DTOs.Research;

public class CreateResearchMilestoneRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? TargetDate { get; set; }
    public int OrderIndex { get; set; }
}

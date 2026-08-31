namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchMilestoneDto
{
    public Guid Id { get; set; }
    public Guid ResearchProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public DateTime? AchievedAt { get; set; }
    public int OrderIndex { get; set; }
    public int TaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

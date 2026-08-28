namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class LearningRoadmapListItemDto
{
    public Guid Id { get; set; }
    public string Goal { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CompletedMilestoneCount { get; set; }
    public int TotalMilestoneCount { get; set; }
    public decimal ProgressPercent { get; set; }
    public DateTime GeneratedAt { get; set; }
}

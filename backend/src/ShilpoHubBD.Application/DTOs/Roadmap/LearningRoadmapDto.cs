namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class LearningRoadmapDto
{
    public Guid Id { get; set; }
    public string Goal { get; set; } = string.Empty;
    public Guid? TargetHeritageSkillId { get; set; }
    public string? TargetHeritageSkillName { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<RoadmapMilestoneDto> Milestones { get; set; } = new();
    public RoadmapMilestoneDto? NextStep { get; set; }
    public int CompletedMilestoneCount { get; set; }
    public int TotalMilestoneCount { get; set; }
    public decimal ProgressPercent { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

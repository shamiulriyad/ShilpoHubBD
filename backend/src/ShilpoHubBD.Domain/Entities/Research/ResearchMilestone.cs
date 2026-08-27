namespace ShilpoHubBD.Domain.Entities.Research;

public class ResearchMilestone
{
    public Guid Id { get; set; }

    public Guid ResearchProjectId { get; set; }
    public ResearchProject Project { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ResearchMilestoneStatus Status { get; set; } = ResearchMilestoneStatus.Planned;
    public DateTime? TargetDate { get; set; }
    public DateTime? AchievedAt { get; set; }
    public int OrderIndex { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ResearchTask> Tasks { get; set; } = new List<ResearchTask>();
}

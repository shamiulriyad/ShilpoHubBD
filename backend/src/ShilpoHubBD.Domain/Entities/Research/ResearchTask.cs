using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Research;

public class ResearchTask
{
    public Guid Id { get; set; }

    public Guid ResearchProjectId { get; set; }
    public ResearchProject Project { get; set; } = null!;

    public Guid? MilestoneId { get; set; }
    public ResearchMilestone? Milestone { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ResearchTaskStatus Status { get; set; } = ResearchTaskStatus.Todo;
    public ResearchTaskPriority Priority { get; set; } = ResearchTaskPriority.Medium;

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

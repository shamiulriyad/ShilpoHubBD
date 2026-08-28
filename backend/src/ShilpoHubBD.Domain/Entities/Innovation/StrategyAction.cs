using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>A timeline action within a preservation strategy, optionally tied to an objective.</summary>
public class StrategyAction
{
    public Guid Id { get; set; }

    public Guid PreservationStrategyId { get; set; }
    public PreservationStrategy Strategy { get; set; } = null!;

    public Guid? StrategyObjectiveId { get; set; }
    public StrategyObjective? Objective { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public StrategyActionStatus Status { get; set; } = StrategyActionStatus.Planned;
    public int OrderIndex { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

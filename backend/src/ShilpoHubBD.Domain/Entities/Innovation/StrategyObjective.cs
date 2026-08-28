namespace ShilpoHubBD.Domain.Entities.Innovation;

public class StrategyObjective
{
    public Guid Id { get; set; }

    public Guid PreservationStrategyId { get; set; }
    public PreservationStrategy Strategy { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }

    public bool IsAchieved { get; set; }
    public DateTime? AchievedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<StrategyAction> Actions { get; set; } = new List<StrategyAction>();
}

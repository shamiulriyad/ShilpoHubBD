namespace ShilpoHubBD.Application.DTOs.Innovation;

public class StrategyObjectiveDto
{
    public Guid Id { get; set; }
    public Guid PreservationStrategyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsAchieved { get; set; }
    public DateTime? AchievedAt { get; set; }
}

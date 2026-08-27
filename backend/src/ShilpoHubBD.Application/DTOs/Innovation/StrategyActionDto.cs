namespace ShilpoHubBD.Application.DTOs.Innovation;

public class StrategyActionDto
{
    public Guid Id { get; set; }
    public Guid PreservationStrategyId { get; set; }
    public Guid? StrategyObjectiveId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.Innovation;

public class UpdateStrategyActionRequest
{
    public Guid? StrategyObjectiveId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

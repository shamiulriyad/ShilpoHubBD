namespace ShilpoHubBD.Application.DTOs.Innovation;

public class PreservationStrategyListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public int ObjectiveCount { get; set; }
    public int ActionCount { get; set; }
    public int CompletedActionCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

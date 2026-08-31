namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchTaskDto
{
    public Guid Id { get; set; }
    public Guid ResearchProjectId { get; set; }
    public Guid? MilestoneId { get; set; }
    public string? MilestoneTitle { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

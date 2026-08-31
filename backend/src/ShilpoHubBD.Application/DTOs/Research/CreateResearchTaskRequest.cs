namespace ShilpoHubBD.Application.DTOs.Research;

public class CreateResearchTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public Guid? MilestoneId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
}

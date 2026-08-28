namespace ShilpoHubBD.Application.DTOs.Assessment;

public class CreateAssignmentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public DateTime? DueAt { get; set; }
}

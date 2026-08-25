namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class MilestoneProgressDto
{
    public Guid MilestoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

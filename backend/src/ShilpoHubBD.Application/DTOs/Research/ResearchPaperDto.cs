namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchPaperDto
{
    public Guid Id { get; set; }
    public Guid ResearchProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public string? Authors { get; set; }
    public string? Keywords { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ManuscriptUrl { get; set; }
    public string? TargetVenue { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

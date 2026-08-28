namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchPublicationDto
{
    public Guid Id { get; set; }
    public Guid ResearchProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public Guid? ResearchPaperId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Doi { get; set; }
    public string? Url { get; set; }
    public string? Abstract { get; set; }
    public string? Citation { get; set; }
    public DateTime? PublishedOn { get; set; }
    public bool IsPublic { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

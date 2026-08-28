namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyListItemDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TargetRegion { get; set; }
    public string Language { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public string MyRole { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int FieldResearcherCount { get; set; }
    public int ResponseCount { get; set; }
    public DateTime? OpensAt { get; set; }
    public DateTime? ClosesAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

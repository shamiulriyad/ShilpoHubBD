namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class UpdateSurveyRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public string? TargetRegion { get; set; }
    public string Language { get; set; } = string.Empty;
    public bool AllowAnonymousResponses { get; set; }
    public Guid? ResearchProjectId { get; set; }
    public DateTime? OpensAt { get; set; }
    public DateTime? ClosesAt { get; set; }
}

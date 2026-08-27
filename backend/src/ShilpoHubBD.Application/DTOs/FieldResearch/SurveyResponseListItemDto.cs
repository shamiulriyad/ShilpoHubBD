namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyResponseListItemDto
{
    public Guid Id { get; set; }
    public Guid SurveyId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? RespondentName { get; set; }
    public string? VillageName { get; set; }
    public string? DistrictName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public string? SubmittedByName { get; set; }
    public int AnswerCount { get; set; }
    public int EvidenceCount { get; set; }
    public DateTime CollectedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

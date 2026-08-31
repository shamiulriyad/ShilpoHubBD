using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyResponseDto
{
    public Guid Id { get; set; }
    public Guid SurveyId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? RespondentName { get; set; }
    public string? RespondentContact { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? LocationAccuracyMeters { get; set; }
    public string? VillageName { get; set; }
    public string? DistrictName { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public string? SubmittedByName { get; set; }
    public string? ReviewNote { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CollectedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SurveyResponseAnswerDto> Answers { get; set; } = new();
    public List<FieldEvidenceDto> Evidence { get; set; } = new();
}

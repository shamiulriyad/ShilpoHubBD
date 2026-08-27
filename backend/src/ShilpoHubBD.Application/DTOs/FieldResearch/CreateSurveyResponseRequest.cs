using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class CreateSurveyResponseRequest
{
    public string? Source { get; set; }
    public string? RespondentName { get; set; }
    public string? RespondentContact { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? LocationAccuracyMeters { get; set; }
    public string? VillageName { get; set; }
    public string? DistrictName { get; set; }
    public DateTime? CollectedAt { get; set; }
    public bool SubmitNow { get; set; }
    public List<SurveyAnswerInputDto> Answers { get; set; } = new();
}

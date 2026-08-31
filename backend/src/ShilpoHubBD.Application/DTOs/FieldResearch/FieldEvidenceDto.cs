namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class FieldEvidenceDto
{
    public Guid Id { get; set; }
    public Guid SurveyId { get; set; }
    public Guid? SurveyResponseId { get; set; }
    public Guid CapturedByUserId { get; set; }
    public string CapturedByName { get; set; } = string.Empty;
    public string EvidenceType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public long? FileSizeBytes { get; set; }
    public int? DurationSeconds { get; set; }
    public string? TranscriptText { get; set; }
    public string? Language { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? LocationAccuracyMeters { get; set; }
    public DateTime CapturedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

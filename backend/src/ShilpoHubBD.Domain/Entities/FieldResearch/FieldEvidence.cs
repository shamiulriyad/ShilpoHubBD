using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.FieldResearch;

/// <summary>
/// Field evidence metadata: photos, audio/voice recordings, interview transcripts, documents and GPS
/// waypoints. The backend stores metadata and file references only -- no audio/video is processed.
/// </summary>
public class FieldEvidence
{
    public Guid Id { get; set; }

    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; } = null!;

    public Guid? SurveyResponseId { get; set; }
    public SurveyResponse? Response { get; set; }

    public Guid CapturedByUserId { get; set; }
    public User CapturedBy { get; set; } = null!;

    public FieldEvidenceType EvidenceType { get; set; } = FieldEvidenceType.Photo;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // File reference only.
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public long? FileSizeBytes { get; set; }
    public int? DurationSeconds { get; set; }

    /// <summary>Transcript / voice-documentation text captured in the field (not machine-generated).</summary>
    public string? TranscriptText { get; set; }
    public string? Language { get; set; }

    // GPS of the evidence.
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? LocationAccuracyMeters { get; set; }

    public DateTime CapturedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.FieldResearch;

/// <summary>A single collected survey response, with GPS + timestamp of where/when it was gathered.</summary>
public class SurveyResponse
{
    public Guid Id { get; set; }

    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; } = null!;

    /// <summary>The field researcher who collected it; null for anonymous / self-reported entries.</summary>
    public Guid? SubmittedByUserId { get; set; }
    public User? SubmittedBy { get; set; }

    public string? RespondentName { get; set; }
    public string? RespondentContact { get; set; }

    public SurveyResponseStatus Status { get; set; } = SurveyResponseStatus.Draft;
    public FieldResponseSource Source { get; set; } = FieldResponseSource.FieldInterview;

    public DateTime CollectedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }

    // GPS of the collection point.
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? LocationAccuracyMeters { get; set; }
    public string? VillageName { get; set; }
    public string? DistrictName { get; set; }

    public string? ReviewNote { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SurveyResponseAnswer> Answers { get; set; } = new List<SurveyResponseAnswer>();
    public ICollection<FieldEvidence> Evidence { get; set; } = new List<FieldEvidence>();
}

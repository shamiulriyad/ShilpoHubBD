namespace ShilpoHubBD.Domain.Entities.FieldResearch;

public class SurveyResponseAnswer
{
    public Guid Id { get; set; }

    public Guid SurveyResponseId { get; set; }
    public SurveyResponse Response { get; set; } = null!;

    public Guid SurveyQuestionId { get; set; }
    public SurveyQuestion Question { get; set; } = null!;

    /// <summary>Free text, or a JSON array for MultipleChoice answers.</summary>
    public string? ValueText { get; set; }
    public double? ValueNumber { get; set; }
    public bool? ValueBoolean { get; set; }
    public DateTime? ValueDate { get; set; }

    // Populated for GpsPoint questions.
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

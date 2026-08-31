namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyResponseAnswerDto
{
    public Guid Id { get; set; }
    public Guid SurveyQuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string? ValueText { get; set; }
    public double? ValueNumber { get; set; }
    public bool? ValueBoolean { get; set; }
    public DateTime? ValueDate { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

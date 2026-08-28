namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyAnswerInputDto
{
    public Guid SurveyQuestionId { get; set; }
    public string? ValueText { get; set; }
    public double? ValueNumber { get; set; }
    public bool? ValueBoolean { get; set; }
    public DateTime? ValueDate { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

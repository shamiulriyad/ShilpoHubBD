namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class CreateSurveyQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public string? HelpText { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int OrderIndex { get; set; }
    public string? OptionsJson { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
}

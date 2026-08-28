namespace ShilpoHubBD.Domain.Entities.FieldResearch;

public class SurveyQuestion
{
    public Guid Id { get; set; }

    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public string? HelpText { get; set; }

    public SurveyQuestionType QuestionType { get; set; } = SurveyQuestionType.ShortText;
    public bool IsRequired { get; set; }
    public int OrderIndex { get; set; }

    /// <summary>JSON array of option labels for SingleChoice / MultipleChoice / Scale questions.</summary>
    public string? OptionsJson { get; set; }

    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SurveyResponseAnswer> Answers { get; set; } = new List<SurveyResponseAnswer>();
}

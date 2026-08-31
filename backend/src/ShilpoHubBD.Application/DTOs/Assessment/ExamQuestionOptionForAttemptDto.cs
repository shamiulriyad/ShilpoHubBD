namespace ShilpoHubBD.Application.DTOs.Assessment;

// Student-facing view of an option while an attempt is in progress — deliberately omits IsCorrect.
public class ExamQuestionOptionForAttemptDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

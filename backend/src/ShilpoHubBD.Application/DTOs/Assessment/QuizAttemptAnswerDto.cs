namespace ShilpoHubBD.Application.DTOs.Assessment;

public class QuizAttemptAnswerDto
{
    public Guid QuestionId { get; set; }
    public string QuestionBody { get; set; } = string.Empty;
    public Guid? SelectedOptionId { get; set; }
    public string? SelectedOptionText { get; set; }
    public Guid? CorrectOptionId { get; set; }
    public string? CorrectOptionText { get; set; }
    public bool? IsCorrect { get; set; }
    public int? PointsAwarded { get; set; }
}

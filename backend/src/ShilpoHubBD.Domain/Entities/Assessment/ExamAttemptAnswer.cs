namespace ShilpoHubBD.Domain.Entities.Assessment;

public class ExamAttemptAnswer
{
    public Guid Id { get; set; }

    public Guid ExamAttemptId { get; set; }
    public ExamAttempt ExamAttempt { get; set; } = null!;

    public Guid ExamQuestionId { get; set; }
    public ExamQuestion ExamQuestion { get; set; } = null!;

    public Guid? SelectedOptionId { get; set; }
    public ExamQuestionOption? SelectedOption { get; set; }

    public string? EssayAnswerText { get; set; }

    public bool? IsCorrect { get; set; }
    public int? PointsAwarded { get; set; }
    public string? Feedback { get; set; }
}

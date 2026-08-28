namespace ShilpoHubBD.Domain.Entities.Assessment;

public class QuizAttemptAnswer
{
    public Guid Id { get; set; }

    public Guid QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = null!;

    public Guid QuizQuestionId { get; set; }
    public QuizQuestion QuizQuestion { get; set; } = null!;

    public Guid? SelectedOptionId { get; set; }
    public QuizQuestionOption? SelectedOption { get; set; }

    public bool? IsCorrect { get; set; }
    public int? PointsAwarded { get; set; }
}

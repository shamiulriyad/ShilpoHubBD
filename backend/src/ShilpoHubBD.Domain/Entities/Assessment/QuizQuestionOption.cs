namespace ShilpoHubBD.Domain.Entities.Assessment;

public class QuizQuestionOption
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }
    public QuizQuestion Question { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
}

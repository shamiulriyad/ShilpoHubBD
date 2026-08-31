namespace ShilpoHubBD.Domain.Entities.Assessment;

public class QuizQuestion
{
    public Guid Id { get; set; }

    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public int Points { get; set; } = 1;
    public int DisplayOrder { get; set; }

    public ICollection<QuizQuestionOption> Options { get; set; } = new List<QuizQuestionOption>();
}

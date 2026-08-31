namespace ShilpoHubBD.Domain.Entities.Assessment;

public class ExamQuestion
{
    public Guid Id { get; set; }

    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    public int Points { get; set; } = 1;
    public int DisplayOrder { get; set; }

    public ICollection<ExamQuestionOption> Options { get; set; } = new List<ExamQuestionOption>();
}

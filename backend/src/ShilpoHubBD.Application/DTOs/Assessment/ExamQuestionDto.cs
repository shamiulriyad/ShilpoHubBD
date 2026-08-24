namespace ShilpoHubBD.Application.DTOs.Assessment;

public class ExamQuestionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string Body { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public int Points { get; set; }
    public int DisplayOrder { get; set; }
    public List<ExamQuestionOptionDto> Options { get; set; } = new();
}

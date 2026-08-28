namespace ShilpoHubBD.Application.DTOs.Assessment;

public class QuizQuestionDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public string Body { get; set; } = string.Empty;
    public int Points { get; set; }
    public int DisplayOrder { get; set; }
    public List<QuizQuestionOptionDto> Options { get; set; } = new();
}

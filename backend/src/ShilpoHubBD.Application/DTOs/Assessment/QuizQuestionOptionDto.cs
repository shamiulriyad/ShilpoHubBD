namespace ShilpoHubBD.Application.DTOs.Assessment;

public class QuizQuestionOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.Assessment;

public class QuizQuestionForAttemptDto
{
    public Guid Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public int Points { get; set; }
    public int DisplayOrder { get; set; }
    public List<QuizQuestionOptionForAttemptDto> Options { get; set; } = new();
}

namespace ShilpoHubBD.Application.DTOs.Assessment;

public class QuizAttemptStartDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public List<QuizQuestionForAttemptDto> Questions { get; set; } = new();
}

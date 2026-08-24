namespace ShilpoHubBD.Application.DTOs.Assessment;

public class ExamAttemptStartDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public List<ExamQuestionForAttemptDto> Questions { get; set; } = new();
}

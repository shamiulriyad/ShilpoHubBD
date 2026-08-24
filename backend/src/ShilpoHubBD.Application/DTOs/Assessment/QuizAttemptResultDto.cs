namespace ShilpoHubBD.Application.DTOs.Assessment;

public class QuizAttemptResultDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public Guid StudentUserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? Score { get; set; }
    public int MaxScore { get; set; }
    public decimal? PercentageScore { get; set; }
    public bool? IsPassed { get; set; }
    public List<QuizAttemptAnswerDto> Answers { get; set; } = new();
}

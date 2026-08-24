namespace ShilpoHubBD.Application.DTOs.Assessment;

public class ExamAttemptResultDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
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
    public DateTime? EvaluatedAt { get; set; }
    public List<ExamAttemptAnswerDto> Answers { get; set; } = new();
}

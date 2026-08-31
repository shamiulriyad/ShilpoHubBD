using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Assessment;

public class ExamAttempt
{
    public Guid Id { get; set; }

    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    public Guid StudentUserId { get; set; }
    public User Student { get; set; } = null!;

    public int AttemptNumber { get; set; }
    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public int? Score { get; set; }
    public int MaxScore { get; set; }
    public decimal? PercentageScore { get; set; }
    public bool? IsPassed { get; set; }

    public DateTime? EvaluatedAt { get; set; }
    public Guid? EvaluatedByUserId { get; set; }

    public ICollection<ExamAttemptAnswer> Answers { get; set; } = new List<ExamAttemptAnswer>();
}

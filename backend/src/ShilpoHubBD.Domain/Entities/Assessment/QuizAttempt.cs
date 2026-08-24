using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Assessment;

public class QuizAttempt
{
    public Guid Id { get; set; }

    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

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

    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}

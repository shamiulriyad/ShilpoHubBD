using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Assessment;

public class Quiz
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? TimeLimitMinutes { get; set; }
    public int? MaxAttempts { get; set; }
    public decimal PassingScorePercentage { get; set; } = 60m;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}

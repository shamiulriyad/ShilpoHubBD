namespace ShilpoHubBD.Application.DTOs.Assessment;

public class QuizListItemDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? TimeLimitMinutes { get; set; }
    public int? MaxAttempts { get; set; }
    public decimal PassingScorePercentage { get; set; }
    public int QuestionCount { get; set; }
    public int TotalPoints { get; set; }
}

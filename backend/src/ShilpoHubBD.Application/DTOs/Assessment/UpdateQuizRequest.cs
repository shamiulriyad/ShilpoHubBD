namespace ShilpoHubBD.Application.DTOs.Assessment;

public class UpdateQuizRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? TimeLimitMinutes { get; set; }
    public int? MaxAttempts { get; set; }
    public decimal PassingScorePercentage { get; set; } = 60m;
}

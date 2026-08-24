namespace ShilpoHubBD.Application.DTOs.SkillAssessment;

// A single normalized data point (a quiz, exam, or assignment result) fed into the assessment provider.
public class PerformanceSignal
{
    public string Title { get; set; } = string.Empty;
    public decimal PercentageScore { get; set; }
    public bool? IsPassed { get; set; }
}

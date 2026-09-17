namespace ShilpoHubBD.Application.DTOs.SentimentAnalysis;

public class SentimentResultDto
{
    /// <summary>"Positive", "Neutral" or "Negative".</summary>
    public string Sentiment { get; set; } = string.Empty;

    /// <summary>-1 (very negative) to 1 (very positive).</summary>
    public decimal Score { get; set; }
}

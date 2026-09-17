namespace ShilpoHubBD.Application.DTOs.SentimentAnalysis;

public class ProductSentimentSummaryDto
{
    public Guid ProductId { get; set; }
    public int ReviewCount { get; set; }
    public decimal AverageScore { get; set; }
    public string OverallSentiment { get; set; } = string.Empty;
    public int PositiveCount { get; set; }
    public int NeutralCount { get; set; }
    public int NegativeCount { get; set; }
}

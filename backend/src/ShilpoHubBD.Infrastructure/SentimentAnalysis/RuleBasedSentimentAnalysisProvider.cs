using System.Text.RegularExpressions;
using ShilpoHubBD.Application.DTOs.SentimentAnalysis;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.SentimentAnalysis;

/// <summary>Lexicon-based sentiment scoring — counts positive/negative word matches, no ML model. Swap
/// in a model-backed <see cref="ISentimentAnalysisProvider"/> later if desired.</summary>
public class RuleBasedSentimentAnalysisProvider : ISentimentAnalysisProvider
{
    private static readonly HashSet<string> PositiveWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "love", "great", "excellent", "amazing", "beautiful", "perfect", "wonderful", "good", "best",
        "happy", "recommend", "quality", "fast", "authentic", "gorgeous", "awesome", "satisfied", "nice",
    };

    private static readonly HashSet<string> NegativeWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "bad", "poor", "terrible", "awful", "worst", "disappointed", "broken", "damaged", "late", "fake",
        "cheap", "scam", "rude", "slow", "defective", "waste", "horrible", "unhappy", "refund",
    };

    private static readonly Regex WordSplitter = new(@"[^a-zA-Z']+", RegexOptions.Compiled);

    public SentimentResultDto Analyze(string text)
    {
        var words = WordSplitter.Split(text ?? string.Empty).Where(w => w.Length > 0).ToList();

        var positiveHits = words.Count(w => PositiveWords.Contains(w));
        var negativeHits = words.Count(w => NegativeWords.Contains(w));
        var totalHits = positiveHits + negativeHits;

        var score = totalHits == 0 ? 0m : (decimal)(positiveHits - negativeHits) / totalHits;

        var sentiment = score switch
        {
            >= 0.2m => "Positive",
            <= -0.2m => "Negative",
            _ => "Neutral",
        };

        return new SentimentResultDto { Sentiment = sentiment, Score = Math.Round(score, 2) };
    }
}

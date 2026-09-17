using ShilpoHubBD.Application.DTOs.SentimentAnalysis;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.SentimentAnalysis;

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private const int MaxReviewsConsidered = 200;

    private readonly IReviewRepository _reviewRepository;
    private readonly ISentimentAnalysisProvider _provider;

    public SentimentAnalysisService(IReviewRepository reviewRepository, ISentimentAnalysisProvider provider)
    {
        _reviewRepository = reviewRepository;
        _provider = provider;
    }

    public Task<SentimentResultDto> AnalyzeAsync(AnalyzeSentimentRequest request, CancellationToken cancellationToken)
        => Task.FromResult(_provider.Analyze(request.Text));

    public async Task<ProductSentimentSummaryDto> GetProductSentimentAsync(Guid productId, CancellationToken cancellationToken)
    {
        var (reviews, totalCount) = await _reviewRepository.GetPagedByProductAsync(
            productId, 1, MaxReviewsConsidered, cancellationToken);

        if (reviews.Count == 0)
        {
            return new ProductSentimentSummaryDto { ProductId = productId, OverallSentiment = "Neutral" };
        }

        var results = reviews.Select(r => _provider.Analyze(r.Comment)).ToList();
        var averageScore = results.Average(r => r.Score);

        return new ProductSentimentSummaryDto
        {
            ProductId = productId,
            ReviewCount = totalCount,
            AverageScore = Math.Round(averageScore, 2),
            OverallSentiment = averageScore switch
            {
                >= 0.2m => "Positive",
                <= -0.2m => "Negative",
                _ => "Neutral",
            },
            PositiveCount = results.Count(r => r.Sentiment == "Positive"),
            NeutralCount = results.Count(r => r.Sentiment == "Neutral"),
            NegativeCount = results.Count(r => r.Sentiment == "Negative"),
        };
    }
}

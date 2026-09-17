using ShilpoHubBD.Application.DTOs.SentimentAnalysis;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISentimentAnalysisService
{
    Task<SentimentResultDto> AnalyzeAsync(AnalyzeSentimentRequest request, CancellationToken cancellationToken);
    Task<ProductSentimentSummaryDto> GetProductSentimentAsync(Guid productId, CancellationToken cancellationToken);
}

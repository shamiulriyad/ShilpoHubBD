using Microsoft.Extensions.Logging;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Services.Reviews;

public class ReviewAiAnalysisService : IReviewAiAnalysisService
{
    private readonly IReviewAiAnalysisRepository _repository;
    private readonly IReviewModerationAIProvider _provider;
    private readonly IRepeatedComplaintDetectionService _repeatedComplaintService;
    private readonly ILogger<ReviewAiAnalysisService> _logger;

    public ReviewAiAnalysisService(
        IReviewAiAnalysisRepository repository,
        IReviewModerationAIProvider provider,
        IRepeatedComplaintDetectionService repeatedComplaintService,
        ILogger<ReviewAiAnalysisService> logger)
    {
        _repository = repository;
        _provider = provider;
        _repeatedComplaintService = repeatedComplaintService;
        _logger = logger;
    }

    public async Task AnalyzeReviewAsync(Review review, Product product, CancellationToken cancellationToken)
    {
        ReviewAiAnalysis analysis;
        try
        {
            // A review is analyzed exactly once; re-running this for the same id must never create a second row.
            if (await _repository.ExistsForReviewAsync(review.Id, cancellationToken))
            {
                return;
            }

            var result = await _provider.AnalyzeAsync(new ReviewModerationContext
            {
                Comment = review.Comment,
                Rating = review.Rating,
                ProductName = product.Name,
            }, cancellationToken);

            var now = DateTime.UtcNow;
            analysis = new ReviewAiAnalysis
            {
                Id = Guid.NewGuid(),
                ReviewId = review.Id,
                IsNegative = result.IsNegative,
                IsProductRelated = result.IsProductRelated,
                ComplaintType = result.ComplaintType,
                Severity = result.Severity,
                IssueSummary = result.IssueSummary,
                Confidence = result.Confidence,
                IsAiGenerated = result.IsAiGenerated,
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _repository.AddAsync(analysis, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exc)
        {
            // The provider already falls back to a rule-based result when Gemini itself fails; this only
            // catches a genuinely unexpected error (e.g. a DB hiccup). Per the failure-handling contract for
            // this feature we do not invent an analysis here — the review is simply left unanalyzed.
            _logger.LogWarning(exc, "Review moderation analysis failed unexpectedly for review {ReviewId}; skipping.", review.Id);
            return;
        }

        if (!analysis.IsNegative)
        {
            return;
        }

        // The analysis is already saved at this point; a failure here must never undo or hide it.
        try
        {
            await _repeatedComplaintService.EvaluateAsync(review, analysis, cancellationToken);
        }
        catch (Exception exc)
        {
            _logger.LogWarning(exc, "Repeated-complaint evaluation failed unexpectedly for review {ReviewId}; skipping.", review.Id);
        }
    }
}

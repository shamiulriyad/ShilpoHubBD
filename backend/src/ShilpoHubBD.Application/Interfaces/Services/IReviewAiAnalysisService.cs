using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>Runs AI moderation analysis for a newly created product review and stores the result. Never throws:
/// any failure is logged and swallowed so a review's creation is never affected by the AI layer. When the
/// analysis is negative, this also chains into <see cref="IRepeatedComplaintDetectionService"/>.</summary>
public interface IReviewAiAnalysisService
{
    Task AnalyzeReviewAsync(Review review, Product product, CancellationToken cancellationToken);
}

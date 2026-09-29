using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>Runs after a review's AI moderation analysis when that analysis is negative: retrieves similar
/// historical reviews for the same product, asks Gemini (or its rule-based fallback) whether this is a
/// repeated complaint, and applies deterministic risk logic. Never throws: any failure is logged and
/// swallowed so it can never affect review creation or storage of the review's own analysis.</summary>
public interface IRepeatedComplaintDetectionService
{
    Task EvaluateAsync(Review review, ReviewAiAnalysis analysis, CancellationToken cancellationToken);
}

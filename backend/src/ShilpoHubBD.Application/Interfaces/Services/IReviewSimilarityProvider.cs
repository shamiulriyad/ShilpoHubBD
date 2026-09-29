using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>Retrieves semantically similar historical reviews from the existing review vector index (RAG).
/// Mirrors <see cref="IProductSearchCandidateProvider"/>: returns null on ANY failure (unavailable service,
/// timeout, malformed response) so the caller falls back to a plain database query instead of failing.</summary>
public interface IReviewSimilarityProvider
{
    Task<List<SimilarReviewMatchDto>?> FindSimilarAsync(Guid productId, string queryText, int limit, CancellationToken cancellationToken);
}

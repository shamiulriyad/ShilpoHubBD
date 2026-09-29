using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>Feeds the review vector index, mirroring <see cref="IProductIndexService"/>. PostgreSQL stays the
/// source of truth; this only hands the Python worker documents to embed and records what it acknowledges.</summary>
public interface IReviewIndexService
{
    Task<ReviewIndexBatchDto> GetPendingAsync(int limit, CancellationToken cancellationToken);
    Task AckAsync(ReviewIndexAckRequest request, CancellationToken cancellationToken);
}

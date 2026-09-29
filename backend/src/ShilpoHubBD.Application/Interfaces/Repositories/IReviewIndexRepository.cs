using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IReviewIndexRepository
{
    Task<List<ReviewIndexState>> GetPendingAsync(int limit, int maxAttempts, CancellationToken cancellationToken);
    Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken);
    Task<Dictionary<Guid, ReviewIndexState>> GetStatesAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken);

    /// <summary>Reviews with their Product (for ProductId/ProducerId) eager-loaded, keyed by review id.</summary>
    Task<Dictionary<Guid, Review>> GetReviewsForIndexAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken);
    Task<Dictionary<Guid, ReviewAiAnalysis>> GetAnalysesAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken);

    void RemoveState(ReviewIndexState state);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

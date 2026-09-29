using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IReviewAiAnalysisRepository
{
    Task<bool> ExistsForReviewAsync(Guid reviewId, CancellationToken cancellationToken);
    Task<ReviewAiAnalysis?> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken);
    Task AddAsync(ReviewAiAnalysis analysis, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

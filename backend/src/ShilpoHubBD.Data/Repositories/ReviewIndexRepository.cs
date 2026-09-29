using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ProductSearch;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Repositories;

public class ReviewIndexRepository : IReviewIndexRepository
{
    private readonly ShilpoHubDbContext _context;

    public ReviewIndexRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // Dirty rows first, then failed ones that still have attempts left.
    private IQueryable<ReviewIndexState> Pending(int maxAttempts)
        => _context.ReviewIndexStates.Where(s => s.Status == IndexStatuses.Dirty || s.Status == IndexStatuses.Deleted
            || (s.Status == IndexStatuses.Failed && s.AttemptCount < maxAttempts));

    public Task<List<ReviewIndexState>> GetPendingAsync(int limit, int maxAttempts, CancellationToken cancellationToken)
        => Pending(maxAttempts).OrderBy(s => s.UpdatedAt).Take(limit).ToListAsync(cancellationToken);

    public Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken) => Pending(maxAttempts).CountAsync(cancellationToken);

    public Task<Dictionary<Guid, ReviewIndexState>> GetStatesAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
        => _context.ReviewIndexStates.Where(s => reviewIds.Contains(s.ReviewId)).ToDictionaryAsync(s => s.ReviewId, cancellationToken);

    public async Task<Dictionary<Guid, Review>> GetReviewsForIndexAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
        => (await _context.Reviews.AsNoTracking()
            .Include(r => r.Product)
            .Where(r => reviewIds.Contains(r.Id))
            .ToListAsync(cancellationToken)).ToDictionary(r => r.Id);

    public async Task<Dictionary<Guid, ReviewAiAnalysis>> GetAnalysesAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
        => (await _context.ReviewAiAnalyses.AsNoTracking()
            .Where(a => reviewIds.Contains(a.ReviewId))
            .ToListAsync(cancellationToken)).ToDictionary(a => a.ReviewId);

    public void RemoveState(ReviewIndexState state) => _context.ReviewIndexStates.Remove(state);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Repositories;

public class ReviewAiAnalysisRepository : IReviewAiAnalysisRepository
{
    private readonly ShilpoHubDbContext _context;

    public ReviewAiAnalysisRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsForReviewAsync(Guid reviewId, CancellationToken cancellationToken)
        => _context.ReviewAiAnalyses.AnyAsync(a => a.ReviewId == reviewId, cancellationToken);

    public Task<ReviewAiAnalysis?> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken)
        => _context.ReviewAiAnalyses.FirstOrDefaultAsync(a => a.ReviewId == reviewId, cancellationToken);

    public async Task AddAsync(ReviewAiAnalysis analysis, CancellationToken cancellationToken)
        => await _context.ReviewAiAnalyses.AddAsync(analysis, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}

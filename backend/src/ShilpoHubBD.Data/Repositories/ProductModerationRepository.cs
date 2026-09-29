using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Repositories;

public class ProductModerationRepository : IProductModerationRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProductModerationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<ProductModerationState> GetOrCreateStateAsync(Guid productId, CancellationToken cancellationToken)
    {
        var state = await _context.ProductModerationStates
            .Include(s => s.Product).ThenInclude(p => p.Producer)
            .FirstOrDefaultAsync(s => s.ProductId == productId, cancellationToken);

        if (state is not null)
        {
            return state;
        }

        var now = DateTime.UtcNow;
        state = new ProductModerationState { ProductId = productId, CreatedAt = now, UpdatedAt = now };
        await _context.ProductModerationStates.AddAsync(state, cancellationToken);

        // The caller needs Product (and its producer) loaded even for a brand-new state row.
        state.Product = await _context.Products.Include(p => p.Producer).FirstAsync(p => p.Id == productId, cancellationToken);
        return state;
    }

    public async Task<List<Guid>> GetRecentNegativeReviewIdsForProductAsync(
        Guid productId, Guid excludeReviewId, int limit, CancellationToken cancellationToken)
        => await _context.Reviews
            .Where(r => r.ProductId == productId && r.Id != excludeReviewId)
            .Join(_context.ReviewAiAnalyses.Where(a => a.IsNegative), r => r.Id, a => a.ReviewId, (r, a) => r)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

    public async Task<Dictionary<Guid, (Review Review, ReviewAiAnalysis? Analysis)>> GetReviewsWithAnalysisByIdsAsync(
        IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
    {
        var reviews = await _context.Reviews.AsNoTracking()
            .Where(r => reviewIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
        var analyses = await _context.ReviewAiAnalyses.AsNoTracking()
            .Where(a => reviewIds.Contains(a.ReviewId))
            .ToDictionaryAsync(a => a.ReviewId, cancellationToken);

        return reviews.ToDictionary(r => r.Id, r => (r, analyses.GetValueOrDefault(r.Id)));
    }

    public Task<List<ProductModerationEvent>> GetRecentEventsAsync(Guid productId, int limit, CancellationToken cancellationToken)
        => _context.ProductModerationEvents
            .Where(e => e.ProductId == productId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public Task<int> CountNegativeReviewsForProductAsync(Guid productId, CancellationToken cancellationToken)
        => _context.Reviews
            .Where(r => r.ProductId == productId)
            .Join(_context.ReviewAiAnalyses.Where(a => a.IsNegative), r => r.Id, a => a.ReviewId, (r, a) => r.Id)
            .CountAsync(cancellationToken);

    public Task<List<ProducerModerationWarning>> GetWarningsForProductAsync(Guid productId, int limit, CancellationToken cancellationToken)
        => _context.ProducerModerationWarnings
            .Where(w => w.ProductId == productId)
            .OrderByDescending(w => w.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<(List<ProductModerationCaseRow> Items, int TotalCount)> GetCasesAsync(
        ProductModerationRiskState? riskState, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.MonitoringFlags.AsNoTracking()
            .Where(f => f.FlagType == MonitoringFlagType.RepeatedProductComplaints)
            .Join(_context.ProductModerationStates.AsNoTracking(), f => f.SubjectId, s => (Guid?)s.ProductId, (f, s) => new { Flag = f, State = s })
            .Join(_context.Products.AsNoTracking(), x => x.State.ProductId, p => p.Id, (x, p) => new { x.Flag, x.State, Product = p })
            .Join(_context.Users.AsNoTracking(), x => x.Product.ProducerId, u => u.Id, (x, u) => new { x.Flag, x.State, x.Product, Producer = u });

        if (riskState.HasValue)
        {
            query = query.Where(x => x.State.RiskState == riskState.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MonitoringFlagStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Flag.Status == parsedStatus);
        }

        var ordered = query.OrderByDescending(x => x.Flag.DetectedAt);
        var totalCount = await ordered.CountAsync(cancellationToken);
        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductModerationCaseRow(
                x.Flag.Id, x.Product.Id, x.Product.Name, x.Producer.Id, x.Producer.FullName,
                x.State.RiskState, x.State.SimilarComplaintCount, x.State.HighSeverityComplaintCount,
                x.State.ProducerWarningCount, x.Flag.Status.ToString(), x.Flag.DetectedAt))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddEventAsync(ProductModerationEvent moderationEvent, CancellationToken cancellationToken)
        => await _context.ProductModerationEvents.AddAsync(moderationEvent, cancellationToken);

    public async Task AddWarningAsync(ProducerModerationWarning warning, CancellationToken cancellationToken)
        => await _context.ProducerModerationWarnings.AddAsync(warning, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}

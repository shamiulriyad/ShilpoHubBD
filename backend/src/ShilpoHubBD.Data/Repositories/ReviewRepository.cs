using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ShilpoHubDbContext _context;

    public ReviewRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Review> WithDetails()
        => _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Images)
            .AsSplitQuery();

    public async Task<(List<Review> Items, int TotalCount)> GetPagedByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var reviews = WithDetails().Where(r => r.ProductId == productId).OrderByDescending(r => r.CreatedAt);

        var totalCount = await reviews.CountAsync(cancellationToken);
        var items = await reviews
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Review?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken cancellationToken)
        => _context.Reviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);

    public async Task<(double AverageRating, int ReviewCount)> GetAggregateAsync(Guid productId, CancellationToken cancellationToken)
    {
        var ratings = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        return ratings.Count == 0 ? (0, 0) : (ratings.Average(), ratings.Count);
    }

    public async Task AddAsync(Review review, CancellationToken cancellationToken)
        => await _context.Reviews.AddAsync(review, cancellationToken);

    public async Task AddImageAsync(ReviewImage image, CancellationToken cancellationToken)
        => await _context.ReviewImages.AddAsync(image, cancellationToken);

    public void RemoveImage(ReviewImage image)
        => _context.ReviewImages.Remove(image);

    public void Remove(Review review)
        => _context.Reviews.Remove(review);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

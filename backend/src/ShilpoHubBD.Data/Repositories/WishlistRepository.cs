using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly ShilpoHubDbContext _context;

    public WishlistRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<WishlistItem> WithDetails()
        => _context.WishlistItems
            .Include(w => w.Product).ThenInclude(p => p.Images)
            .AsSplitQuery();

    public Task<List<WishlistItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<WishlistItem?> GetAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

    public async Task AddAsync(WishlistItem item, CancellationToken cancellationToken)
        => await _context.WishlistItems.AddAsync(item, cancellationToken);

    public void Remove(WishlistItem item)
        => _context.WishlistItems.Remove(item);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

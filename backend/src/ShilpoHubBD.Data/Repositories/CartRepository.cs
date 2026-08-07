using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ShilpoHubDbContext _context;

    public CartRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<CartItem> WithDetails()
        => _context.CartItems
            .Include(c => c.Product).ThenInclude(p => p.Images)
            .Include(c => c.ProductVariant)
            .AsSplitQuery();

    public Task<List<CartItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<CartItem?> GetAsync(Guid userId, Guid productId, Guid? productVariantId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(
            c => c.UserId == userId && c.ProductId == productId && c.ProductVariantId == productVariantId,
            cancellationToken);

    public Task<CartItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(CartItem item, CancellationToken cancellationToken)
        => await _context.CartItems.AddAsync(item, cancellationToken);

    public void Remove(CartItem item)
        => _context.CartItems.Remove(item);

    public async Task ClearAsync(Guid userId, CancellationToken cancellationToken)
        => await _context.CartItems.Where(c => c.UserId == userId).ExecuteDeleteAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

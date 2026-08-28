using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Inventory;

namespace ShilpoHubBD.Data.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ShilpoHubDbContext _context;

    public InventoryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<InventoryTransaction>> GetByProductAsync(Guid productId, CancellationToken cancellationToken)
        => _context.InventoryTransactions
            .Include(t => t.ProductVariant)
            .Include(t => t.CreatedBy)
            .Where(t => t.ProductId == productId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(InventoryTransaction transaction, CancellationToken cancellationToken)
        => await _context.InventoryTransactions.AddAsync(transaction, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

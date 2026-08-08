using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Traceability;

namespace ShilpoHubBD.Data.Repositories;

public class TraceabilityRepository : ITraceabilityRepository
{
    private readonly ShilpoHubDbContext _context;

    public TraceabilityRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProductTraceability> WithDetails()
        => _context.ProductTraceabilities
            .Include(t => t.Product)
            .Include(t => t.MaterialSources)
            .Include(t => t.TimelineEvents)
            .AsSplitQuery();

    public Task<ProductTraceability?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<ProductTraceability?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(t => t.ProductId == productId, cancellationToken);

    public Task<bool> ExistsByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        => _context.ProductTraceabilities.AnyAsync(t => t.ProductId == productId, cancellationToken);

    public async Task AddAsync(ProductTraceability traceability, CancellationToken cancellationToken)
        => await _context.ProductTraceabilities.AddAsync(traceability, cancellationToken);

    public void Remove(ProductTraceability traceability)
        => _context.ProductTraceabilities.Remove(traceability);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

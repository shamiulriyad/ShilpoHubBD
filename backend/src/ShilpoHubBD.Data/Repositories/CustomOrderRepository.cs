using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.CustomOrders;

namespace ShilpoHubBD.Data.Repositories;

public class CustomOrderRepository : ICustomOrderRepository
{
    private readonly ShilpoHubDbContext _context;

    public CustomOrderRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<CustomOrderRequest> WithDetails()
        => _context.CustomOrderRequests
            .Include(c => c.Producer)
            .Include(c => c.Customer)
            .Include(c => c.Product)
            .AsSplitQuery();

    public Task<CustomOrderRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<CustomOrderRequest>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(c => c.ProducerId == producerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<List<CustomOrderRequest>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CustomOrderRequest request, CancellationToken cancellationToken)
        => await _context.CustomOrderRequests.AddAsync(request, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

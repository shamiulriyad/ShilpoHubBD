using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Repositories;

public class OrderComplaintRepository : IOrderComplaintRepository
{
    private readonly ShilpoHubDbContext _context;

    public OrderComplaintRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<OrderComplaint> WithDetails()
        => _context.OrderComplaints
            .Include(c => c.Order)
            .Include(c => c.Product)
            .Include(c => c.Producer)
            .Include(c => c.Customer);

    public Task<OrderComplaint?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<OrderComplaint>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        => WithDetails().Where(c => c.CustomerId == customerId).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);

    public Task<List<OrderComplaint>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken)
        => WithDetails().Where(c => c.ProducerId == producerId).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);

    public Task<bool> HasUnsettledAsync(Guid customerId, Guid productId, CancellationToken cancellationToken)
        => _context.OrderComplaints.AnyAsync(c => c.CustomerId == customerId && c.ProductId == productId
            && (c.Status == OrderComplaintStatus.Open || c.Status == OrderComplaintStatus.Resolved), cancellationToken);

    public Task<bool> HasOpenForOrderProductAsync(Guid orderId, Guid productId, CancellationToken cancellationToken)
        => _context.OrderComplaints.AnyAsync(c => c.OrderId == orderId && c.ProductId == productId
            && (c.Status == OrderComplaintStatus.Open || c.Status == OrderComplaintStatus.Resolved), cancellationToken);

    public async Task AddAsync(OrderComplaint complaint, CancellationToken cancellationToken)
        => await _context.OrderComplaints.AddAsync(complaint, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

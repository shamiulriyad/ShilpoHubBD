using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ShilpoHubDbContext _context;

    public PaymentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Payment> WithDetails()
        => _context.Payments.Include(p => p.Order);

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasActivePaymentAsync(Guid orderId, CancellationToken cancellationToken)
        => _context.Payments.AnyAsync(
            p => p.OrderId == orderId &&
                (p.Status == PaymentStatus.Awaiting || p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.PartiallyRefunded),
            cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
        => await _context.Payments.AddAsync(payment, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

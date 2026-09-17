using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Commerce;
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

    public async Task<(List<Payment> Items, int TotalCount)> GetPagedAsync(PaymentAdminQueryParameters query, CancellationToken cancellationToken)
    {
        var payments = WithDetails();

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<PaymentStatus>(query.Status, true, out var status))
        {
            payments = payments.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            payments = payments.Where(p =>
                EF.Functions.ILike(p.Order.OrderNumber, term) || EF.Functions.ILike(p.Order.RecipientName, term));
        }

        if (query.From.HasValue)
        {
            payments = payments.Where(p => p.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            payments = payments.Where(p => p.CreatedAt <= query.To.Value);
        }

        payments = payments.OrderByDescending(p => p.CreatedAt);

        var totalCount = await payments.CountAsync(cancellationToken);
        var items = await payments
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
        => await _context.Payments.AddAsync(payment, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

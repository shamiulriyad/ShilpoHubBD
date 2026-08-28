using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerOrderRepository : IProducerOrderRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerOrderRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<OrderItem> WithDetails()
        => _context.OrderItems
            .Include(i => i.Order)
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .AsSplitQuery();

    public async Task<(List<OrderItem> Items, int TotalCount)> GetPagedByProducerAsync(
        Guid producerId, OrderItemProducerStatus? status, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken cancellationToken)
    {
        var items = WithDetails().Where(i => i.Product.ProducerId == producerId);

        if (status.HasValue)
        {
            items = items.Where(i => i.ProducerStatus == status.Value);
        }

        if (fromDate.HasValue)
        {
            items = items.Where(i => i.Order.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            items = items.Where(i => i.Order.CreatedAt <= toDate.Value);
        }

        items = items.OrderByDescending(i => i.Order.CreatedAt);

        var totalCount = await items.CountAsync(cancellationToken);
        var paged = await items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (paged, totalCount);
    }

    public Task<OrderItem?> GetByIdAsync(Guid orderItemId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(i => i.Id == orderItemId, cancellationToken);

    public async Task<List<OrderItem>> GetByProducerAsync(
        Guid producerId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        var items = WithDetails().Where(i => i.Product.ProducerId == producerId);

        if (fromDate.HasValue)
        {
            items = items.Where(i => i.Order.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            items = items.Where(i => i.Order.CreatedAt <= toDate.Value);
        }

        return await items.ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, (string FullName, string Email)>> GetCustomerInfoAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken)
        => await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToDictionaryAsync(u => u.Id, u => (u.FullName, u.Email), cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

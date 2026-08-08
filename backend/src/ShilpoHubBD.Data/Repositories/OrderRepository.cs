using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ShilpoHubDbContext _context;

    public OrderRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Order> WithDetails()
        => _context.Orders
            .Include(o => o.ShippingDistrict)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .Include(o => o.StatusHistory)
            .AsSplitQuery();

    public async Task<(List<Order> Items, int TotalCount)> GetPagedByUserAsync(Guid userId, OrderQueryParameters query, CancellationToken cancellationToken)
    {
        var orders = WithDetails().Where(o => o.UserId == userId);

        if (query.Status.HasValue)
        {
            orders = orders.Where(o => o.Status == query.Status.Value);
        }

        orders = orders.OrderByDescending(o => o.CreatedAt);

        var totalCount = await orders.CountAsync(cancellationToken);
        var items = await orders
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<bool> ExistsByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken)
        => _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);

    public Task<bool> HasPurchasedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
        => _context.Orders
            .Where(o => o.UserId == userId && (
                o.Status == OrderStatus.Delivered ||
                o.Status == OrderStatus.ReturnRequested ||
                o.Status == OrderStatus.Returned ||
                o.Status == OrderStatus.Refunded))
            .SelectMany(o => o.Items)
            .AnyAsync(i => i.ProductId == productId, cancellationToken);

    public Task<bool> HasCompletedOrderFromDistrictAsync(Guid userId, Guid districtId, CancellationToken cancellationToken)
        => _context.Orders
            .Where(o => o.UserId == userId && (
                o.Status == OrderStatus.Delivered ||
                o.Status == OrderStatus.ReturnRequested ||
                o.Status == OrderStatus.Returned ||
                o.Status == OrderStatus.Refunded))
            .SelectMany(o => o.Items)
            .AnyAsync(i => i.Product.DistrictId == districtId, cancellationToken);

    public Task<int> GetCompletedOrderCountAsync(Guid userId, CancellationToken cancellationToken)
        => _context.Orders.CountAsync(o => o.UserId == userId && (
            o.Status == OrderStatus.Delivered ||
            o.Status == OrderStatus.ReturnRequested ||
            o.Status == OrderStatus.Returned ||
            o.Status == OrderStatus.Refunded), cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
        => await _context.Orders.AddAsync(order, cancellationToken);

    public async Task AddStatusEventAsync(OrderStatusEvent statusEvent, CancellationToken cancellationToken)
        => await _context.OrderStatusEvents.AddAsync(statusEvent, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Analytics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private static readonly OrderStatus[] CompletedStatuses =
    {
        OrderStatus.Delivered,
        OrderStatus.ReturnRequested,
        OrderStatus.Returned,
        OrderStatus.Refunded,
    };

    private readonly ShilpoHubDbContext _context;

    public AnalyticsRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Order> CompletedOrders(Guid userId)
        => _context.Orders.Where(o => o.UserId == userId && CompletedStatuses.Contains(o.Status));

    public async Task<PurchaseAnalyticsDto> GetPurchaseAnalyticsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var orders = CompletedOrders(userId);

        var totalOrders = await orders.CountAsync(cancellationToken);
        if (totalOrders == 0)
        {
            return new PurchaseAnalyticsDto();
        }

        var totalSpent = await orders.SumAsync(o => o.Total, cancellationToken);
        var totalItems = await orders.SelectMany(o => o.Items).SumAsync(i => i.Quantity, cancellationToken);
        var firstPurchaseAt = await orders.MinAsync(o => o.CreatedAt, cancellationToken);
        var lastPurchaseAt = await orders.MaxAsync(o => o.CreatedAt, cancellationToken);

        return new PurchaseAnalyticsDto
        {
            TotalOrders = totalOrders,
            TotalItemsPurchased = totalItems,
            TotalSpent = totalSpent,
            AverageOrderValue = totalSpent / totalOrders,
            FirstPurchaseAt = firstPurchaseAt,
            LastPurchaseAt = lastPurchaseAt,
        };
    }

    public async Task<List<SpendingByMonthDto>> GetSpendingByMonthAsync(Guid userId, int months, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        return await CompletedOrders(userId)
            .Where(o => o.CreatedAt >= cutoff)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new SpendingByMonthDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalSpent = g.Sum(o => o.Total),
                OrderCount = g.Count(),
            })
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FavoriteCategoryDto>> GetFavoriteCategoriesAsync(Guid userId, int count, CancellationToken cancellationToken)
    {
        return await CompletedOrders(userId)
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.Product.CategoryId, i.Product.Category.Name })
            .Select(g => new FavoriteCategoryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                ItemCount = g.Sum(i => i.Quantity),
                TotalSpent = g.Sum(i => i.LineTotal),
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}

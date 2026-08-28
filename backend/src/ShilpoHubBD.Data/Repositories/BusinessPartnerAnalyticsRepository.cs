using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Data.Repositories;

public class BusinessPartnerAnalyticsRepository : IBusinessPartnerAnalyticsRepository
{
    private static readonly OrderStatus[] ActiveOrderStatuses =
    {
        OrderStatus.Pending,
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.Delivered,
    };

    private readonly ShilpoHubDbContext _context;

    public BusinessPartnerAnalyticsRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<OrderItem> ActiveOrderItems(AnalyticsQueryParameters parameters)
    {
        var query = _context.OrderItems.Where(oi => ActiveOrderStatuses.Contains(oi.Order.Status));

        if (parameters.DateFrom.HasValue)
        {
            query = query.Where(oi => oi.Order.CreatedAt >= parameters.DateFrom.Value);
        }

        if (parameters.DateTo.HasValue)
        {
            query = query.Where(oi => oi.Order.CreatedAt <= parameters.DateTo.Value);
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(oi => oi.Product.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.DistrictId.HasValue)
        {
            query = query.Where(oi => oi.Order.ShippingDistrictId == parameters.DistrictId.Value);
        }

        return query;
    }

    public Task<List<CategoryDemandDto>> GetMarketDemandAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
        => ActiveOrderItems(parameters)
            .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
            .Select(g => new CategoryDemandDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalQuantityOrdered = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.LineTotal),
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count(),
            })
            .OrderByDescending(d => d.TotalQuantityOrdered)
            .ToListAsync(cancellationToken);

    public async Task<List<MonthlyTrendDto>> GetExportTrendsAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var items = await ActiveOrderItems(parameters)
            .Select(oi => new { oi.Quantity, oi.LineTotal, OrderCreatedAt = oi.Order.CreatedAt })
            .ToListAsync(cancellationToken);

        return GroupByMonth(items, i => i.OrderCreatedAt, i => i.Quantity, i => i.LineTotal);
    }

    public async Task<List<MonthlyTrendDto>> GetCategoryMonthlyQuantityAsync(Guid categoryId, int months, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var items = await _context.OrderItems
            .Where(oi => ActiveOrderStatuses.Contains(oi.Order.Status) && oi.Product.CategoryId == categoryId && oi.Order.CreatedAt >= cutoff)
            .Select(oi => new { oi.Quantity, oi.LineTotal, OrderCreatedAt = oi.Order.CreatedAt })
            .ToListAsync(cancellationToken);

        return GroupByMonth(items, i => i.OrderCreatedAt, i => i.Quantity, i => i.LineTotal);
    }

    public async Task<List<IndustryInsightDto>> GetIndustryInsightsAsync(AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var profileQuery = _context.BusinessPartnerProfiles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(parameters.Industry))
        {
            profileQuery = profileQuery.Where(p => p.Industry == parameters.Industry);
        }

        var profiles = await profileQuery
            .Select(p => new { p.UserId, p.Industry })
            .ToListAsync(cancellationToken);

        var businessPartnerIds = profiles.Select(p => p.UserId).ToList();

        var orderQuery = _context.Orders.Where(o => businessPartnerIds.Contains(o.UserId) && ActiveOrderStatuses.Contains(o.Status));
        if (parameters.DateFrom.HasValue)
        {
            orderQuery = orderQuery.Where(o => o.CreatedAt >= parameters.DateFrom.Value);
        }

        if (parameters.DateTo.HasValue)
        {
            orderQuery = orderQuery.Where(o => o.CreatedAt <= parameters.DateTo.Value);
        }

        var orders = await orderQuery.Select(o => new { o.UserId, o.Total }).ToListAsync(cancellationToken);
        var ordersByUser = orders.ToLookup(o => o.UserId);

        return profiles
            .GroupBy(p => p.Industry)
            .Select(g =>
            {
                var industryOrders = g.SelectMany(p => ordersByUser[p.UserId]).ToList();
                var totalSpending = industryOrders.Sum(o => o.Total);
                return new IndustryInsightDto
                {
                    Industry = g.Key,
                    BusinessPartnerCount = g.Count(),
                    TotalOrders = industryOrders.Count,
                    TotalSpending = totalSpending,
                    AverageOrderValue = industryOrders.Count == 0 ? 0 : Math.Round(totalSpending / industryOrders.Count, 2),
                };
            })
            .OrderByDescending(i => i.TotalSpending)
            .ToList();
    }

    public async Task<ProcurementAnalyticsDto> GetProcurementAnalyticsAsync(
        Guid? businessPartnerId, AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.ProcurementRequests.Include(p => p.Items).AsQueryable();

        if (businessPartnerId.HasValue)
        {
            query = query.Where(p => p.BusinessPartnerId == businessPartnerId.Value);
        }

        if (parameters.DateFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= parameters.DateFrom.Value);
        }

        if (parameters.DateTo.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= parameters.DateTo.Value);
        }

        var requests = await query.ToListAsync(cancellationToken);

        var statusBreakdown = requests
            .GroupBy(r => r.Status)
            .Select(g => new ProcurementStatusBreakdownDto
            {
                Status = g.Key,
                Count = g.Count(),
                TotalValue = g.Sum(r => r.Items.Sum(i => i.UnitPrice * i.Quantity)),
            })
            .OrderByDescending(b => b.Count)
            .ToList();

        var approvedWithDates = requests.Where(r => r.ApprovedAt.HasValue).ToList();

        return new ProcurementAnalyticsDto
        {
            TotalRequests = requests.Count,
            TotalValue = requests.Sum(r => r.Items.Sum(i => i.UnitPrice * i.Quantity)),
            ApprovedCount = requests.Count(r => r.Status is ProcurementStatus.Approved or ProcurementStatus.Converted),
            RejectedCount = requests.Count(r => r.Status == ProcurementStatus.Rejected),
            ConvertedCount = requests.Count(r => r.Status == ProcurementStatus.Converted),
            AverageApprovalDays = approvedWithDates.Count == 0
                ? null
                : approvedWithDates.Average(r => (r.ApprovedAt!.Value - r.CreatedAt).TotalDays),
            StatusBreakdown = statusBreakdown,
        };
    }

    public async Task<List<SupplierPerformanceDto>> GetSupplierPerformanceAsync(
        Guid? businessPartnerId, AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.ProcurementRequests
            .Include(p => p.Producer)
            .Include(p => p.Items)
            .AsQueryable();

        if (businessPartnerId.HasValue)
        {
            query = query.Where(p => p.BusinessPartnerId == businessPartnerId.Value);
        }

        if (parameters.DateFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= parameters.DateFrom.Value);
        }

        if (parameters.DateTo.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= parameters.DateTo.Value);
        }

        var requests = await query.ToListAsync(cancellationToken);
        var producerIds = requests.Select(r => r.ProducerId).Distinct().ToList();

        var productAggregates = await _context.Products
            .Where(p => producerIds.Contains(p.ProducerId) && p.IsActive)
            .GroupBy(p => p.ProducerId)
            .Select(g => new
            {
                ProducerId = g.Key,
                TotalReviewCount = g.Sum(p => p.ReviewCount),
                WeightedRatingSum = g.Sum(p => p.AverageRating * p.ReviewCount),
            })
            .ToListAsync(cancellationToken);
        var ratingByProducer = productAggregates.ToDictionary(a => a.ProducerId, a => a);

        var deliveredItems = await _context.OrderItems
            .Where(oi => producerIds.Contains(oi.Product.ProducerId) && oi.ProducerStatus == OrderItemProducerStatus.Delivered && oi.DeliveredAt.HasValue)
            .Select(oi => new { ProducerId = oi.Product.ProducerId, oi.DeliveredAt, OrderCreatedAt = oi.Order.CreatedAt })
            .ToListAsync(cancellationToken);
        var deliveryByProducer = deliveredItems
            .GroupBy(i => i.ProducerId)
            .ToDictionary(g => g.Key, g => g.Average(i => (i.DeliveredAt!.Value - i.OrderCreatedAt).TotalDays));

        return requests
            .GroupBy(r => new { r.ProducerId, ProducerName = r.Producer.FullName })
            .Select(g =>
            {
                ratingByProducer.TryGetValue(g.Key.ProducerId, out var rating);
                deliveryByProducer.TryGetValue(g.Key.ProducerId, out var avgDelivery);

                return new SupplierPerformanceDto
                {
                    ProducerId = g.Key.ProducerId,
                    ProducerName = g.Key.ProducerName,
                    AverageRating = rating is null || rating.TotalReviewCount == 0
                        ? 0m
                        : Math.Round(rating.WeightedRatingSum / rating.TotalReviewCount, 2),
                    TotalProcurements = g.Count(),
                    CompletedProcurements = g.Count(r => r.Status == ProcurementStatus.Converted),
                    CancelledProcurements = g.Count(r => r.Status == ProcurementStatus.Cancelled),
                    TotalProcurementValue = g.Sum(r => r.Items.Sum(i => i.UnitPrice * i.Quantity)),
                    AverageDeliveryDays = deliveryByProducer.ContainsKey(g.Key.ProducerId) ? Math.Round(avgDelivery, 1) : null,
                };
            })
            .OrderByDescending(s => s.AverageRating)
            .ToList();
    }

    public async Task<SpendingAnalyticsDto> GetSpendingAnalyticsAsync(
        Guid? businessPartnerId, AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.Orders.Where(o => ActiveOrderStatuses.Contains(o.Status));

        if (businessPartnerId.HasValue)
        {
            query = query.Where(o => o.UserId == businessPartnerId.Value);
        }

        if (parameters.DateFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= parameters.DateFrom.Value);
        }

        if (parameters.DateTo.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= parameters.DateTo.Value);
        }

        var orders = await query
            .Select(o => new { o.Id, o.Total, o.CreatedAt })
            .ToListAsync(cancellationToken);

        var orderIds = orders.Select(o => o.Id).ToList();
        var items = await _context.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .Select(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category.Name, oi.LineTotal })
            .ToListAsync(cancellationToken);

        var totalSpent = orders.Sum(o => o.Total);

        return new SpendingAnalyticsDto
        {
            TotalSpent = totalSpent,
            TotalOrders = orders.Count,
            AverageOrderValue = orders.Count == 0 ? 0 : Math.Round(totalSpent / orders.Count, 2),
            MonthlySpending = GroupByMonth(orders, o => o.CreatedAt, _ => 1, o => o.Total),
            SpendingByCategory = items
                .GroupBy(i => new { i.CategoryId, i.CategoryName })
                .Select(g => new CategorySpendingDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalSpent = g.Sum(i => i.LineTotal),
                    OrderCount = g.Count(),
                })
                .OrderByDescending(c => c.TotalSpent)
                .ToList(),
        };
    }

    public async Task<List<MonthlyTrendDto>> GetOrderTrendsAsync(
        Guid? businessPartnerId, AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.Orders.AsQueryable();

        if (businessPartnerId.HasValue)
        {
            query = query.Where(o => o.UserId == businessPartnerId.Value);
        }

        if (parameters.DateFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= parameters.DateFrom.Value);
        }

        if (parameters.DateTo.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= parameters.DateTo.Value);
        }

        var orders = await query.Select(o => new { o.Total, o.CreatedAt }).ToListAsync(cancellationToken);

        return GroupByMonth(orders, o => o.CreatedAt, _ => 1, o => o.Total);
    }

    private static List<MonthlyTrendDto> GroupByMonth<T>(
        List<T> items, Func<T, DateTime> dateSelector, Func<T, int> quantitySelector, Func<T, decimal> valueSelector)
        => items
            .GroupBy(i => new DateTime(dateSelector(i).Year, dateSelector(i).Month, 1))
            .Select(g => new MonthlyTrendDto
            {
                PeriodStart = g.Key,
                Quantity = g.Sum(quantitySelector),
                Value = g.Sum(valueSelector),
            })
            .OrderBy(t => t.PeriodStart)
            .ToList();
}

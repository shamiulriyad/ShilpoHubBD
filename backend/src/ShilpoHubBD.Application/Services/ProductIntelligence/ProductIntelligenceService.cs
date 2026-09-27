using ShilpoHubBD.Application.DTOs.ProductIntelligence;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Services.ProductIntelligence;

public class ProductIntelligenceService : IProductIntelligenceService
{
    // Same convention as ProducerOrderService/AIBusinessService: only a delivered item is realized
    // revenue/sales, so cancelled/rejected/in-flight items never inflate the trend.
    private static readonly OrderItemProducerStatus[] RevenueStatuses = { OrderItemProducerStatus.Delivered };

    private readonly IProductRepository _productRepository;
    private readonly IProducerOrderRepository _producerOrderRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductIntelligenceAIProvider _aiProvider;

    public ProductIntelligenceService(
        IProductRepository productRepository, IProducerOrderRepository producerOrderRepository, IReviewRepository reviewRepository,
        IWishlistRepository wishlistRepository, IInventoryRepository inventoryRepository, IProductIntelligenceAIProvider aiProvider)
    {
        _productRepository = productRepository;
        _producerOrderRepository = producerOrderRepository;
        _reviewRepository = reviewRepository;
        _wishlistRepository = wishlistRepository;
        _inventoryRepository = inventoryRepository;
        _aiProvider = aiProvider;
    }

    public async Task<ProductIntelligenceDto> GetIntelligenceAsync(Guid productId, ProductIntelligenceRange range, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        var now = DateTime.UtcNow;
        var (rangeStart, bucketByMonth) = ResolveRange(range, now);

        var orderItems = await _producerOrderRepository.GetByProducerAsync(product.ProducerId, rangeStart, now, cancellationToken);
        var productOrderItems = orderItems.Where(i => i.ProductId == productId).ToList();
        var revenueItems = productOrderItems.Where(i => RevenueStatuses.Contains(i.ProducerStatus)).ToList();

        var (reviews, _) = await _reviewRepository.GetPagedByProductAsync(productId, 1, 2000, cancellationToken);
        var reviewsInRange = reviews.Where(r => r.CreatedAt >= rangeStart && r.CreatedAt <= now).ToList();

        var wishlistItems = await _wishlistRepository.GetByProductAsync(productId, cancellationToken);
        var wishlistInRange = wishlistItems.Where(w => w.CreatedAt >= rangeStart && w.CreatedAt <= now).ToList();

        var inventoryTransactions = await _inventoryRepository.GetByProductAsync(productId, cancellationToken);
        var inventoryInRange = inventoryTransactions.Where(t => t.CreatedAt >= rangeStart && t.CreatedAt <= now).ToList();

        var bucketKeys = BuildBucketKeys(rangeStart, now, bucketByMonth);

        var periods = bucketKeys.Select(bucketStart =>
        {
            var bucketEnd = bucketByMonth ? bucketStart.AddMonths(1) : bucketStart.AddDays(1);
            var salesInBucket = revenueItems.Where(i => i.Order.CreatedAt >= bucketStart && i.Order.CreatedAt < bucketEnd).ToList();

            return new ProductIntelligencePeriodDto
            {
                PeriodStart = bucketStart,
                PeriodLabel = bucketByMonth ? bucketStart.ToString("MMM yyyy") : bucketStart.ToString("MMM d"),
                UnitsSold = salesInBucket.Sum(i => i.Quantity),
                OrderCount = salesInBucket.Select(i => i.OrderId).Distinct().Count(),
                Revenue = salesInBucket.Sum(i => i.LineTotal),
                NewReviews = reviewsInRange.Count(r => r.CreatedAt >= bucketStart && r.CreatedAt < bucketEnd),
                WishlistAdds = wishlistInRange.Count(w => w.CreatedAt >= bucketStart && w.CreatedAt < bucketEnd),
            };
        }).ToList();

        var inventoryMovements = bucketKeys.Select(bucketStart =>
        {
            var bucketEnd = bucketByMonth ? bucketStart.AddMonths(1) : bucketStart.AddDays(1);
            var inBucket = inventoryInRange.Where(t => t.CreatedAt >= bucketStart && t.CreatedAt < bucketEnd).ToList();

            return new ProductIntelligenceInventoryMovementDto
            {
                PeriodStart = bucketStart,
                PeriodLabel = bucketByMonth ? bucketStart.ToString("MMM yyyy") : bucketStart.ToString("MMM d"),
                NetChange = inBucket.Sum(t => t.ChangeAmount),
                RestockCount = inBucket.Count(t => t.ChangeAmount > 0),
                SaleDeductionCount = inBucket.Count(t => t.ChangeAmount < 0),
            };
        }).ToList();

        var summary = BuildSummary(periods, rangeStart, now);
        var hasSufficientHistory = periods.Sum(p => p.OrderCount) > 0;

        return new ProductIntelligenceDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProducerId = product.ProducerId,
            ProducerName = product.Producer?.FullName ?? string.Empty,
            CategoryName = product.Category?.Name,
            Range = range,
            RangeStart = rangeStart,
            RangeEnd = now,
            AverageRating = product.AverageRating,
            BayesianRating = product.BayesianRating,
            TotalReviewCount = product.ReviewCount,
            TotalViews = product.ViewCount,
            CurrentWishlistCount = wishlistItems.Count,
            SearchInterestAvailable = false,
            SearchInterestUnavailableReason = "No search-query log exists in this system yet — search interest cannot be reported without inventing data.",
            Periods = periods,
            Inventory = new ProductIntelligenceInventoryDto
            {
                CurrentStock = product.Stock,
                LowStockThreshold = product.LowStockThreshold,
                IsLowStock = product.LowStockThreshold.HasValue && product.Stock <= product.LowStockThreshold.Value,
                Movements = inventoryMovements,
            },
            Summary = summary,
            HasSufficientHistory = hasSufficientHistory,
        };
    }

    public async Task<ProductIntelligenceAiInsightsDto> GetAiInsightsAsync(Guid productId, ProductIntelligenceRange range, CancellationToken cancellationToken)
    {
        var data = await GetIntelligenceAsync(productId, range, cancellationToken);

        var context = new ProductIntelligenceAiContext
        {
            ProductName = data.ProductName,
            CategoryName = data.CategoryName,
            RangeLabel = range.ToString(),
            HasSufficientHistory = data.HasSufficientHistory,
            Periods = data.Periods,
            AverageRating = data.AverageRating,
            TotalReviewCount = data.TotalReviewCount,
            CurrentStock = data.Inventory.CurrentStock,
            LowStockThreshold = data.Inventory.LowStockThreshold,
            RevenueGrowthPercent = data.Summary.RevenueGrowthPercent,
            UnitsGrowthPercent = data.Summary.UnitsGrowthPercent,
        };

        return await _aiProvider.GenerateInsightsAsync(context, cancellationToken);
    }

    private static (DateTime RangeStart, bool BucketByMonth) ResolveRange(ProductIntelligenceRange range, DateTime now) => range switch
    {
        ProductIntelligenceRange.Last30Days => (now.Date.AddDays(-30), false),
        ProductIntelligenceRange.Last3Months => (now.Date.AddMonths(-3), true),
        ProductIntelligenceRange.Last5Months => (now.Date.AddMonths(-5), true),
        ProductIntelligenceRange.Last12Months => (now.Date.AddMonths(-12), true),
        _ => (now.Date.AddDays(-30), false),
    };

    private static List<DateTime> BuildBucketKeys(DateTime rangeStart, DateTime now, bool bucketByMonth)
    {
        var keys = new List<DateTime>();
        if (bucketByMonth)
        {
            var cursor = new DateTime(rangeStart.Year, rangeStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            while (cursor <= end)
            {
                keys.Add(cursor);
                cursor = cursor.AddMonths(1);
            }
        }
        else
        {
            var cursor = DateTime.SpecifyKind(rangeStart.Date, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
            while (cursor <= end)
            {
                keys.Add(cursor);
                cursor = cursor.AddDays(1);
            }
        }

        return keys;
    }

    // The selected range split into two equal halves — the same "recent vs. prior" growth pattern
    // used elsewhere (ProducerBusinessProfileDto), just generalized to whatever range was picked.
    private static ProductIntelligenceSummaryDto BuildSummary(List<ProductIntelligencePeriodDto> periods, DateTime rangeStart, DateTime now)
    {
        var midpoint = rangeStart.AddTicks((now - rangeStart).Ticks / 2);
        var prior = periods.Where(p => p.PeriodStart < midpoint).ToList();
        var recent = periods.Where(p => p.PeriodStart >= midpoint).ToList();

        var recentRevenue = recent.Sum(p => p.Revenue);
        var priorRevenue = prior.Sum(p => p.Revenue);
        var recentUnits = recent.Sum(p => p.UnitsSold);
        var priorUnits = prior.Sum(p => p.UnitsSold);

        return new ProductIntelligenceSummaryDto
        {
            RecentRevenue = recentRevenue,
            PriorRevenue = priorRevenue,
            RevenueGrowthPercent = priorRevenue == 0 ? (recentRevenue == 0 ? null : 100m) : Math.Round((recentRevenue - priorRevenue) / priorRevenue * 100, 2),
            RecentUnitsSold = recentUnits,
            PriorUnitsSold = priorUnits,
            UnitsGrowthPercent = priorUnits == 0 ? (recentUnits == 0 ? null : 100m) : Math.Round((recentUnits - priorUnits) * 100m / priorUnits, 2),
        };
    }
}

// Regression checks for Part 6 Business Partner Product Intelligence: real-data aggregation
// (sales/order/revenue/review/wishlist trend, inventory, growth), graceful handling of insufficient
// history and no-sales products, and that the AI layer only ever receives the already-computed
// aggregated numbers (never a raw DB read) — the grounding requirement. No database — in-memory
// fakes only. Run with `dotnet run`.
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.DTOs.ProductIntelligence;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.ProductIntelligence;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Inventory;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

var producer = new User { Id = Guid.NewGuid(), FullName = "Producer A", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
var category = new Category { Id = Guid.NewGuid(), Name = "Textiles", Slug = "textiles" };

Product MakeProduct(string name, int stock, int? lowStockThreshold, int viewCount, int reviewCount, decimal averageRating)
    => new()
    {
        Id = Guid.NewGuid(), Name = name, Slug = name.ToLowerInvariant().Replace(' ', '-'), ProducerId = producer.Id, Producer = producer,
        CategoryId = category.Id, Category = category, Stock = stock, LowStockThreshold = lowStockThreshold,
        ViewCount = viewCount, ReviewCount = reviewCount, AverageRating = averageRating,
    };

OrderItem MakeOrderItem(Guid productId, decimal unitPrice, int quantity, OrderItemProducerStatus status, DateTime orderCreatedAt)
{
    var order = new Order { Id = Guid.NewGuid(), OrderNumber = $"ORD-{Guid.NewGuid():N}", CreatedAt = orderCreatedAt, UpdatedAt = orderCreatedAt };
    return new OrderItem
    {
        Id = Guid.NewGuid(), OrderId = order.Id, Order = order, ProductId = productId, Product = new Product { Id = productId, ProducerId = producer.Id },
        ProductName = "x", UnitPrice = unitPrice, Quantity = quantity, LineTotal = unitPrice * quantity, ProducerStatus = status,
    };
}

// ===================== Product with real sales history =====================
var nakshiKantha = MakeProduct("Nakshi Kantha", stock: 5, lowStockThreshold: 10, viewCount: 500, reviewCount: 12, averageRating: 4.5m);

var now = DateTime.UtcNow;
var orderRepo = new FakeProducerOrderRepository();
// Prior half of a 30-day range: fewer sales. Recent half: more sales -> should show growth.
orderRepo.AddItems(
    MakeOrderItem(nakshiKantha.Id, 1000m, 2, OrderItemProducerStatus.Delivered, now.AddDays(-28)),
    MakeOrderItem(nakshiKantha.Id, 1000m, 1, OrderItemProducerStatus.Delivered, now.AddDays(-25)),
    MakeOrderItem(nakshiKantha.Id, 1000m, 3, OrderItemProducerStatus.Delivered, now.AddDays(-10)),
    MakeOrderItem(nakshiKantha.Id, 1000m, 4, OrderItemProducerStatus.Delivered, now.AddDays(-3)),
    // Not delivered / cancelled -> must not count.
    MakeOrderItem(nakshiKantha.Id, 1000m, 10, OrderItemProducerStatus.Processing, now.AddDays(-5)),
    MakeOrderItem(nakshiKantha.Id, 1000m, 10, OrderItemProducerStatus.Cancelled, now.AddDays(-4)),
    // A different product's order in the same range must never leak into Nakshi Kantha's numbers.
    MakeOrderItem(Guid.NewGuid(), 5000m, 1, OrderItemProducerStatus.Delivered, now.AddDays(-2))
);

var reviewRepo = new FakeReviewRepository();
reviewRepo.AddReviews(nakshiKantha.Id, now.AddDays(-20), now.AddDays(-2));

var wishlistRepo = new FakeWishlistRepository();
wishlistRepo.AddWishlistAdds(nakshiKantha.Id, now.AddDays(-15), now.AddDays(-1));

var inventoryRepo = new FakeInventoryRepository();
inventoryRepo.AddTransactions(nakshiKantha.Id,
    (now.AddDays(-27), +20, "Restock"),
    (now.AddDays(-10), -3, "Sale"),
    (now.AddDays(-3), -4, "Sale"));

var productRepo = new FakeProductRepository();
productRepo.Add(nakshiKantha);

var capturingAiProvider = new CapturingAiProvider();
var service = new ProductIntelligenceService(productRepo, orderRepo, reviewRepo, wishlistRepo, inventoryRepo, capturingAiProvider);

var data = await service.GetIntelligenceAsync(nakshiKantha.Id, ProductIntelligenceRange.Last30Days, CancellationToken.None);

Check("product name and producer come from the real product record", data.ProductName == "Nakshi Kantha" && data.ProducerId == producer.Id);
Check("total units sold across delivered items only (2+1+3+4=10)", data.Periods.Sum(p => p.UnitsSold) == 10);
Check("total revenue matches delivered items only (10,000)", data.Periods.Sum(p => p.Revenue) == 10000m);
Check("orders from a different product never leak into this product's numbers", data.Periods.Sum(p => p.Revenue) == 10000m);
Check("recent half shows more units than the prior half -> positive growth", data.Summary.UnitsGrowthPercent > 0);
Check("has sufficient history because delivered orders exist", data.HasSufficientHistory);
Check("current stock and low-stock threshold come from the real product record", data.Inventory.CurrentStock == 5 && data.Inventory.LowStockThreshold == 10);
Check("low-stock flag is set because stock (5) <= threshold (10)", data.Inventory.IsLowStock);
Check("inventory movement is captured from real InventoryTransaction rows", data.Inventory.Movements.Sum(m => m.NetChange) == 20 - 3 - 4);
Check("review trend reflects real Review rows in range", data.Periods.Sum(p => p.NewReviews) == 2);
Check("wishlist trend reflects real WishlistItem rows in range", data.Periods.Sum(p => p.WishlistAdds) == 2);
Check("search interest is honestly reported as unavailable, not invented", !data.SearchInterestAvailable && !string.IsNullOrWhiteSpace(data.SearchInterestUnavailableReason));
Check("every day in the 30-day range appears as a bucket, even with zero activity (continuous series)", data.Periods.Count == 31);

// ===================== AI grounding: the provider receives only the pre-computed numbers =====================
await service.GetAiInsightsAsync(nakshiKantha.Id, ProductIntelligenceRange.Last30Days, CancellationToken.None);
Check("the AI provider was called exactly once", capturingAiProvider.CallCount == 1);
Check("the AI context's totals match the already-computed DTO -- no independent DB read on the AI side",
    capturingAiProvider.LastContext!.Periods.Sum(p => p.UnitsSold) == data.Periods.Sum(p => p.UnitsSold)
    && capturingAiProvider.LastContext.AverageRating == data.AverageRating
    && capturingAiProvider.LastContext.CurrentStock == data.Inventory.CurrentStock
    && capturingAiProvider.LastContext.RevenueGrowthPercent == data.Summary.RevenueGrowthPercent);

// ===================== Product with no sales at all =====================
var newProduct = MakeProduct("Brand New Basket", stock: 20, lowStockThreshold: 5, viewCount: 3, reviewCount: 0, averageRating: 0);
productRepo.Add(newProduct);

var noSalesData = await service.GetIntelligenceAsync(newProduct.Id, ProductIntelligenceRange.Last30Days, CancellationToken.None);
Check("a product with zero orders is handled gracefully, not with an error", noSalesData.Periods.Sum(p => p.UnitsSold) == 0);
Check("HasSufficientHistory is false for a product with no delivered orders", !noSalesData.HasSufficientHistory);
Check("growth is null (not a misleading 0% or divide-by-zero) when there's no prior data at all", noSalesData.Summary.RevenueGrowthPercent == null);

var noSalesInsights = await service.GetAiInsightsAsync(newProduct.Id, ProductIntelligenceRange.Last30Days, CancellationToken.None);
Check("insufficient-history is passed through to the AI context truthfully", !capturingAiProvider.LastContext!.HasSufficientHistory);

// ===================== Longer range buckets by month, not by day =====================
var fiveMonthData = await service.GetIntelligenceAsync(nakshiKantha.Id, ProductIntelligenceRange.Last5Months, CancellationToken.None);
Check("a 5-month range buckets by month (6 buckets: current + 5 prior), not by day", fiveMonthData.Periods.Count == 6);

// ===================== Dummy (rule-based) provider never fabricates a number outside the given data =====================
var dummyProvider = new ShilpoHubBD.Infrastructure.ProductIntelligence.DummyProductIntelligenceAIProvider();
var dummyContext = new ProductIntelligenceAiContext
{
    ProductName = "Test", RangeLabel = "Last30Days", HasSufficientHistory = false, Periods = new(), CurrentStock = 5,
};
var dummyResult = await dummyProvider.GenerateInsightsAsync(dummyContext, CancellationToken.None);
Check("the dummy provider clearly marks its output as not AI-generated", !dummyResult.IsAiGenerated);
Check("the dummy provider states insufficient data plainly rather than guessing", dummyResult.DemandTrend == "Insufficient data");

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

class CapturingAiProvider : IProductIntelligenceAIProvider
{
    public int CallCount { get; private set; }
    public ProductIntelligenceAiContext? LastContext { get; private set; }

    public Task<ProductIntelligenceAiInsightsDto> GenerateInsightsAsync(ProductIntelligenceAiContext context, CancellationToken ct)
    {
        CallCount++;
        LastContext = context;
        return Task.FromResult(new ProductIntelligenceAiInsightsDto { IsAiGenerated = true });
    }
}

class FakeProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = new();
    public void Add(Product product) => _products[product.Id] = product;

    public Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query, CancellationToken ct) => Task.FromResult((new List<Product>(), 0));
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(_products.GetValueOrDefault(id));
    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct) => Task.FromResult<Product?>(null);
    public Task<List<Product>> GetFeaturedAsync(int count, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task<List<Product>> GetTrendingAsync(int count, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task<(List<Product> Items, int TotalCount)> GetPendingApprovalAsync(int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<Product>(), 0));
    public Task<(decimal AveragePrice, int SampleSize)> GetCategoryPriceStatsAsync(Guid categoryId, CancellationToken ct) => Task.FromResult((0m, 0));
    public Task<List<Product>> GetByProducerAsync(Guid producerId, CancellationToken ct) => Task.FromResult(_products.Values.Where(p => p.ProducerId == producerId).ToList());
    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct) => Task.FromResult(false);
    public Task<List<Product>> GetLowStockByProducerAsync(Guid producerId, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task AddAsync(Product product, CancellationToken ct) { _products[product.Id] = product; return Task.CompletedTask; }
    public Task AddVariantAsync(ProductVariant variant, CancellationToken ct) => Task.CompletedTask;
    public Task AddVideoAsync(ProductVideo video, CancellationToken ct) => Task.CompletedTask;
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeProducerOrderRepository : IProducerOrderRepository
{
    private readonly List<OrderItem> _items = new();
    public void AddItems(params OrderItem[] items) => _items.AddRange(items);

    public Task<(List<OrderItem> Items, int TotalCount)> GetPagedByProducerAsync(Guid producerId, OrderItemProducerStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct)
        => Task.FromResult((new List<OrderItem>(), 0));
    public Task<OrderItem?> GetByIdAsync(Guid orderItemId, CancellationToken ct) => Task.FromResult<OrderItem?>(null);

    public Task<List<OrderItem>> GetByProducerAsync(Guid producerId, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        var query = _items.Where(i => i.Product.ProducerId == producerId);
        if (fromDate.HasValue) query = query.Where(i => i.Order.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(i => i.Order.CreatedAt <= toDate.Value);
        return Task.FromResult(query.ToList());
    }

    public Task<Dictionary<Guid, (string FullName, string Email)>> GetCustomerInfoAsync(IEnumerable<Guid> userIds, CancellationToken ct) => Task.FromResult(new Dictionary<Guid, (string, string)>());
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeReviewRepository : IReviewRepository
{
    private readonly List<Review> _reviews = new();
    public void AddReviews(Guid productId, params DateTime[] createdAts)
    {
        foreach (var at in createdAts)
        {
            _reviews.Add(new Review { Id = Guid.NewGuid(), ProductId = productId, Rating = 5, CreatedAt = at });
        }
    }

    public Task<(List<Review> Items, int TotalCount)> GetPagedByProductAsync(Guid productId, int page, int pageSize, CancellationToken ct)
    {
        var items = _reviews.Where(r => r.ProductId == productId).ToList();
        return Task.FromResult((items, items.Count));
    }

    public Task<(List<Review> Items, int TotalCount)> GetPagedByHeritagePlaceAsync(Guid heritagePlaceId, int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<Review>(), 0));
    public Task<(List<Review> Items, int TotalCount)> GetPagedByServiceAsync(Guid touristServiceId, int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<Review>(), 0));
    public Task<Review?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult<Review?>(null);
    public Task<Review?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken ct) => Task.FromResult<Review?>(null);
    public Task<Review?> GetByHeritagePlaceAndUserAsync(Guid heritagePlaceId, Guid userId, CancellationToken ct) => Task.FromResult<Review?>(null);
    public Task<Review?> GetByBookingAndUserAsync(Guid bookingId, Guid userId, CancellationToken ct) => Task.FromResult<Review?>(null);
    public Task<(double AverageRating, int ReviewCount)> GetAggregateAsync(Guid productId, CancellationToken ct) => Task.FromResult((0d, 0));
    public Task<(double AverageRating, int ReviewCount)> GetAggregateByHeritagePlaceAsync(Guid heritagePlaceId, CancellationToken ct) => Task.FromResult((0d, 0));
    public Task<(double AverageRating, int ReviewCount)> GetAggregateByServiceAsync(Guid touristServiceId, CancellationToken ct) => Task.FromResult((0d, 0));
    public Task AddAsync(Review review, CancellationToken ct) => Task.CompletedTask;
    public Task AddImageAsync(ReviewImage image, CancellationToken ct) => Task.CompletedTask;
    public void RemoveImage(ReviewImage image) { }
    public void Remove(Review review) { }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeWishlistRepository : IWishlistRepository
{
    private readonly List<WishlistItem> _items = new();
    public void AddWishlistAdds(Guid productId, params DateTime[] createdAts)
    {
        foreach (var at in createdAts)
        {
            _items.Add(new WishlistItem { Id = Guid.NewGuid(), ProductId = productId, UserId = Guid.NewGuid(), CreatedAt = at });
        }
    }

    public Task<List<WishlistItem>> GetByUserIdAsync(Guid userId, CancellationToken ct) => Task.FromResult(new List<WishlistItem>());
    public Task<List<WishlistItem>> GetByProductAsync(Guid productId, CancellationToken ct) => Task.FromResult(_items.Where(w => w.ProductId == productId).ToList());
    public Task<WishlistItem?> GetAsync(Guid userId, Guid productId, CancellationToken ct) => Task.FromResult<WishlistItem?>(null);
    public Task AddAsync(WishlistItem item, CancellationToken ct) => Task.CompletedTask;
    public void Remove(WishlistItem item) { }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeInventoryRepository : IInventoryRepository
{
    private readonly List<InventoryTransaction> _transactions = new();
    public void AddTransactions(Guid productId, params (DateTime At, int Change, string Reason)[] entries)
    {
        foreach (var (at, change, reason) in entries)
        {
            _transactions.Add(new InventoryTransaction { Id = Guid.NewGuid(), ProductId = productId, ChangeAmount = change, Reason = reason, CreatedAt = at, CreatedByUserId = Guid.NewGuid() });
        }
    }

    public Task<List<InventoryTransaction>> GetByProductAsync(Guid productId, CancellationToken ct) => Task.FromResult(_transactions.Where(t => t.ProductId == productId).ToList());
    public Task AddAsync(InventoryTransaction transaction, CancellationToken ct) => Task.CompletedTask;
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

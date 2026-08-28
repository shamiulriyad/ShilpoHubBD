using ShilpoHubBD.Application.DTOs.AIBusiness;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.AIBusiness;

public class AIBusinessService : IAIBusinessService
{
    // Statuses where the producer has committed to the sale; mirrors the revenue rule used by
    // ProducerOrderService so forecasts and dashboards agree on what counts as "sold".
    private static readonly OrderItemProducerStatus[] RevenueStatuses =
    {
        OrderItemProducerStatus.Accepted,
        OrderItemProducerStatus.Processing,
        OrderItemProducerStatus.Shipped,
        OrderItemProducerStatus.Delivered,
    };

    private readonly IAIBusinessProvider _aiBusinessProvider;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProducerOrderRepository _producerOrderRepository;
    private readonly IProducerOrderService _producerOrderService;

    public AIBusinessService(
        IAIBusinessProvider aiBusinessProvider,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProducerOrderRepository producerOrderRepository,
        IProducerOrderService producerOrderService)
    {
        _aiBusinessProvider = aiBusinessProvider;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _producerOrderRepository = producerOrderRepository;
        _producerOrderService = producerOrderService;
    }

    public async Task<PriceSuggestionResult> SuggestPriceAsync(
        Guid producerId, PriceSuggestionRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        Product? product = null;
        if (request.ProductId.HasValue)
        {
            product = await GetOwnedProductAsync(producerId, request.ProductId.Value, cancellationToken);
        }

        var (categoryItems, _) = await _productRepository.GetPagedAsync(
            new ProductQueryParameters { CategoryId = request.CategoryId, PageSize = 200 }, cancellationToken);

        var producerProducts = await _productRepository.GetByProducerAsync(producerId, cancellationToken);

        var context = new PriceSuggestionContext
        {
            ProductName = product?.Name ?? "New product",
            CategoryName = category.Name,
            CurrentPrice = product?.Price,
            CategoryAveragePrice = categoryItems.Count == 0 ? null : categoryItems.Average(p => p.Price),
            CategoryMinPrice = categoryItems.Count == 0 ? null : categoryItems.Min(p => p.Price),
            CategoryMaxPrice = categoryItems.Count == 0 ? null : categoryItems.Max(p => p.Price),
            ProducerAveragePrice = producerProducts.Count == 0 ? null : producerProducts.Average(p => p.Price),
            EstimatedCost = request.EstimatedCost,
            DesiredMarginPercent = request.DesiredMarginPercent,
        };

        return await _aiBusinessProvider.SuggestPriceAsync(context, cancellationToken);
    }

    public async Task<ProductDescriptionResult> GenerateDescriptionAsync(
        Guid producerId, ProductDescriptionRequest request, CancellationToken cancellationToken)
    {
        var context = new ProductDescriptionContext
        {
            ProductName = request.ProductName,
            CategoryName = request.CategoryName ?? string.Empty,
            Keywords = request.Keywords,
            Tone = string.IsNullOrWhiteSpace(request.Tone) ? "Warm" : request.Tone,
        };

        if (request.ProductId.HasValue)
        {
            var product = await GetOwnedProductAsync(producerId, request.ProductId.Value, cancellationToken);
            context.ProductName = product.Name;
            context.CategoryName = product.Category.Name;
            context.DistrictName = product.District.Name;
        }

        return await _aiBusinessProvider.GenerateDescriptionAsync(context, cancellationToken);
    }

    public Task<BusinessTranslationResult> TranslateAsync(BusinessTranslationRequest request, CancellationToken cancellationToken)
        => _aiBusinessProvider.TranslateAsync(request, cancellationToken);

    public async Task<DemandForecastResult> ForecastDemandAsync(
        Guid producerId, DemandForecastRequest request, CancellationToken cancellationToken)
    {
        var product = await GetOwnedProductAsync(producerId, request.ProductId, cancellationToken);

        var horizonWeeks = Math.Clamp(request.HorizonWeeks, 1, 26);
        var lookbackWeeks = Math.Max(12, horizonWeeks * 3);
        var fromDate = DateTime.UtcNow.AddDays(-lookbackWeeks * 7);

        var items = await _producerOrderRepository.GetByProducerAsync(producerId, fromDate, null, cancellationToken);
        var productItems = items
            .Where(i => i.ProductId == request.ProductId && RevenueStatuses.Contains(i.ProducerStatus))
            .ToList();

        var weeklySales = productItems
            .GroupBy(i => GetWeekStart(i.Order.CreatedAt))
            .Select(g => new PeriodQuantityDto { PeriodStart = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .OrderBy(p => p.PeriodStart)
            .ToList();

        var context = new DemandForecastContext
        {
            ProductName = product.Name,
            CurrentStock = product.Stock,
            HorizonWeeks = horizonWeeks,
            HistoricalWeeklySales = weeklySales,
        };

        return await _aiBusinessProvider.ForecastDemandAsync(context, cancellationToken);
    }

    public async Task<ProductionPlanResult> PlanProductionAsync(
        Guid producerId, ProductionPlannerRequest request, CancellationToken cancellationToken)
    {
        var product = await GetOwnedProductAsync(producerId, request.ProductId, cancellationToken);

        var context = new ProductionPlannerContext
        {
            ProductName = product.Name,
            CurrentStock = product.Stock,
            TargetQuantity = Math.Max(0, request.TargetQuantity),
            DailyProductionCapacity = Math.Max(0, request.DailyProductionCapacity),
            LeadTimeDays = Math.Max(0, request.LeadTimeDays),
        };

        return await _aiBusinessProvider.PlanProductionAsync(context, cancellationToken);
    }

    public Task<MaterialForecastResult> ForecastMaterialsAsync(MaterialForecastRequest request, CancellationToken cancellationToken)
        => _aiBusinessProvider.ForecastMaterialsAsync(request, cancellationToken);

    public async Task<SeasonalPredictionResult> PredictSeasonalTrendAsync(
        Guid producerId, SeasonalPredictionRequest request, CancellationToken cancellationToken)
    {
        Guid categoryId;
        string categoryName;

        if (request.ProductId.HasValue)
        {
            var product = await GetOwnedProductAsync(producerId, request.ProductId.Value, cancellationToken);
            categoryId = product.CategoryId;
            categoryName = product.Category.Name;
        }
        else if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new NotFoundException("Category not found.");
            categoryId = category.Id;
            categoryName = category.Name;
        }
        else
        {
            throw new ConflictException("Either ProductId or CategoryId must be provided.");
        }

        var items = await _producerOrderRepository.GetByProducerAsync(producerId, null, null, cancellationToken);
        var categoryItems = items
            .Where(i => i.Product.CategoryId == categoryId && RevenueStatuses.Contains(i.ProducerStatus))
            .ToList();

        var monthlySales = categoryItems
            .GroupBy(i => new DateTime(i.Order.CreatedAt.Year, i.Order.CreatedAt.Month, 1))
            .Select(g => new PeriodQuantityDto { PeriodStart = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .OrderBy(p => p.PeriodStart)
            .ToList();

        var context = new SeasonalPredictionContext { CategoryName = categoryName, HistoricalMonthlySales = monthlySales };
        return await _aiBusinessProvider.PredictSeasonalTrendAsync(context, cancellationToken);
    }

    public async Task<SalesInsightsResult> GenerateSalesInsightsAsync(
        Guid producerId, SalesInsightsRequest request, CancellationToken cancellationToken)
    {
        var revenue = await _producerOrderService.GetRevenueDashboardAsync(producerId, request.FromDate, request.ToDate, cancellationToken);
        var sales = await _producerOrderService.GetSalesAnalyticsAsync(producerId, request.FromDate, request.ToDate, 5, cancellationToken);
        var productPerformance = await _producerOrderService.GetProductPerformanceAsync(producerId, cancellationToken);

        var context = new SalesInsightsContext { Revenue = revenue, Sales = sales, ProductPerformance = productPerformance };
        return await _aiBusinessProvider.GenerateSalesInsightsAsync(context, cancellationToken);
    }

    private async Task<Product> GetOwnedProductAsync(Guid producerId, Guid productId, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this product.");
        }

        return product;
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        date = date.Date;
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}

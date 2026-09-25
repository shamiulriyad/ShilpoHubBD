using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Services.ProducerBusiness;

public class ProducerOrderService : IProducerOrderService
{
    // Revenue and completed orders only count once the logistics partner has delivered the parcel
    // (Delivered is set from the shipment, never by the producer).
    private static readonly OrderItemProducerStatus[] RevenueStatuses =
    {
        OrderItemProducerStatus.Delivered,
    };

    private readonly IProducerOrderRepository _producerOrderRepository;
    private readonly IProductRepository _productRepository;

    private readonly IOrderService _orderService;
    private readonly IDeliveryTrackingService _deliveryTrackingService;

    public ProducerOrderService(
        IProducerOrderRepository producerOrderRepository, IProductRepository productRepository, IOrderService orderService,
        IDeliveryTrackingService deliveryTrackingService)
    {
        _producerOrderRepository = producerOrderRepository;
        _productRepository = productRepository;
        _orderService = orderService;
        _deliveryTrackingService = deliveryTrackingService;
    }

    public async Task<PagedResult<ProducerOrderItemDto>> GetOrdersAsync(
        Guid producerId, ProducerOrderItemQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _producerOrderRepository.GetPagedByProducerAsync(
            producerId, query.Status, query.FromDate, query.ToDate, query.Page, query.PageSize, cancellationToken);

        var customerInfo = await _producerOrderRepository.GetCustomerInfoAsync(
            items.Select(i => i.Order.UserId).Distinct(), cancellationToken);

        return new PagedResult<ProducerOrderItemDto>
        {
            Items = items.Select(i => ToDto(i, customerInfo)).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ProducerOrderItemDto> GetOrderItemAsync(Guid producerId, Guid orderItemId, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(producerId, orderItemId, cancellationToken);
        return await ToDtoWithCustomerAsync(item, cancellationToken);
    }

    public async Task<ProducerOrderItemDto> AcceptAsync(Guid producerId, Guid orderItemId, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(producerId, orderItemId, cancellationToken);
        EnsureStatus(item, OrderItemProducerStatus.Pending, "accepted");

        item.ProducerStatus = OrderItemProducerStatus.Accepted;
        item.ProducerRespondedAt = DateTime.UtcNow;

        await _producerOrderRepository.SaveChangesAsync(cancellationToken);
        await _orderService.SyncStatusFromFulfillmentAsync(item.OrderId, cancellationToken);
        return await ToDtoWithCustomerAsync(item, cancellationToken);
    }

    public async Task<ProducerOrderItemDto> RejectAsync(
        Guid producerId, Guid orderItemId, RejectOrderItemRequest request, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(producerId, orderItemId, cancellationToken);
        EnsureStatus(item, OrderItemProducerStatus.Pending, "rejected");

        item.ProducerStatus = OrderItemProducerStatus.Rejected;
        item.ProducerNote = request.Reason;
        item.ProducerRespondedAt = DateTime.UtcNow;

        await _producerOrderRepository.SaveChangesAsync(cancellationToken);
        await _orderService.SyncStatusFromFulfillmentAsync(item.OrderId, cancellationToken);
        return await ToDtoWithCustomerAsync(item, cancellationToken);
    }

    public async Task<ProducerOrderItemDto> StartProcessingAsync(Guid producerId, Guid orderItemId, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(producerId, orderItemId, cancellationToken);
        EnsureStatus(item, OrderItemProducerStatus.Accepted, "moved to processing");

        item.ProducerStatus = OrderItemProducerStatus.Processing;

        await _producerOrderRepository.SaveChangesAsync(cancellationToken);
        await _orderService.SyncStatusFromFulfillmentAsync(item.OrderId, cancellationToken);
        return await ToDtoWithCustomerAsync(item, cancellationToken);
    }

    public async Task<ProducerOrderItemDto> ShipAsync(
        Guid producerId, Guid orderItemId, ShipOrderItemRequest request, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(producerId, orderItemId, cancellationToken);
        EnsureStatus(item, OrderItemProducerStatus.Processing, "shipped");

        if (!request.LogisticsPartnerProfileId.HasValue)
        {
            throw new ConflictException("Choose a logistics partner to hand this order to. Producers cannot deliver orders themselves.");
        }

        // Hand the item to the chosen logistics partner: they get a shipment for this order and
        // the customer sees the partner and the shipment tracking number.
        var shipment = await _deliveryTrackingService.CreateForOrderHandoffAsync(
            request.LogisticsPartnerProfileId.Value, producerId, item.Product?.Producer?.FullName ?? "Producer",
            new OrderHandoffDetails
            {
                OrderId = item.OrderId,
                RecipientName = item.Order.RecipientName,
                RecipientPhone = item.Order.RecipientPhone,
                DestinationAddressLine = item.Order.ShippingAddressLine,
                DestinationCity = item.Order.ShippingDistrict?.Name ?? "Bangladesh",
                DestinationDistrictId = item.Order.ShippingDistrictId,
                ParcelCount = Math.Max(1, item.Quantity),
                DeclaredValue = item.LineTotal,
                Description = $"{item.Quantity} x {item.ProductName}",
                DeliveryRouteId = request.DeliveryRouteId,
                WeightKg = request.WeightKg,
                Notes = request.Notes,
            },
            cancellationToken);
        var trackingNumber = shipment.TrackingNumber;
        var carrier = shipment.LogisticsPartnerName ?? "Logistics partner";

        item.ProducerStatus = OrderItemProducerStatus.Shipped;
        item.TrackingNumber = trackingNumber;
        item.Carrier = carrier;
        item.ShippedAt = DateTime.UtcNow;

        await _producerOrderRepository.SaveChangesAsync(cancellationToken);
        await _orderService.SyncStatusFromFulfillmentAsync(item.OrderId, cancellationToken);
        return await ToDtoWithCustomerAsync(item, cancellationToken);
    }

    public async Task<List<ProducerCustomerDto>> GetCustomersAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var items = await _producerOrderRepository.GetByProducerAsync(producerId, null, null, cancellationToken);
        var revenueItems = items.Where(i => RevenueStatuses.Contains(i.ProducerStatus)).ToList();

        var customerInfo = await _producerOrderRepository.GetCustomerInfoAsync(
            revenueItems.Select(i => i.Order.UserId).Distinct(), cancellationToken);

        return revenueItems
            .GroupBy(i => i.Order.UserId)
            .Select(g =>
            {
                var (fullName, email) = customerInfo.TryGetValue(g.Key, out var info) ? info : ("Unknown", string.Empty);
                return new ProducerCustomerDto
                {
                    CustomerId = g.Key,
                    CustomerName = fullName,
                    Email = email,
                    TotalOrders = g.Select(i => i.OrderId).Distinct().Count(),
                    TotalItemsPurchased = g.Sum(i => i.Quantity),
                    TotalSpent = g.Sum(i => i.LineTotal),
                    FirstOrderAt = g.Min(i => i.Order.CreatedAt),
                    LastOrderAt = g.Max(i => i.Order.CreatedAt),
                };
            })
            .OrderByDescending(c => c.TotalSpent)
            .ToList();
    }

    public async Task<RevenueDashboardDto> GetRevenueDashboardAsync(
        Guid producerId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        var items = await _producerOrderRepository.GetByProducerAsync(producerId, fromDate, toDate, cancellationToken);
        var revenueItems = items.Where(i => RevenueStatuses.Contains(i.ProducerStatus)).ToList();

        var totalOrders = revenueItems.Select(i => i.OrderId).Distinct().Count();
        var totalRevenue = revenueItems.Sum(i => i.LineTotal);

        return new RevenueDashboardDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalItemsSold = revenueItems.Sum(i => i.Quantity),
            AverageOrderValue = totalOrders == 0 ? 0 : totalRevenue / totalOrders,
            PendingCount = items.Count(i => i.ProducerStatus == OrderItemProducerStatus.Pending),
            AcceptedCount = items.Count(i => i.ProducerStatus == OrderItemProducerStatus.Accepted),
            ProcessingCount = items.Count(i => i.ProducerStatus == OrderItemProducerStatus.Processing),
            ShippedCount = items.Count(i => i.ProducerStatus == OrderItemProducerStatus.Shipped),
            DeliveredCount = items.Count(i => i.ProducerStatus == OrderItemProducerStatus.Delivered),
            RejectedCount = items.Count(i => i.ProducerStatus == OrderItemProducerStatus.Rejected),
            CancelledCount = items.Count(i => i.ProducerStatus == OrderItemProducerStatus.Cancelled),
        };
    }

    public async Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(
        Guid producerId, DateTime? fromDate, DateTime? toDate, int topProductCount, CancellationToken cancellationToken)
    {
        var items = await _producerOrderRepository.GetByProducerAsync(producerId, fromDate, toDate, cancellationToken);
        var revenueItems = items.Where(i => RevenueStatuses.Contains(i.ProducerStatus)).ToList();

        var topProducts = revenueItems
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new ProductSalesDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal),
            })
            .OrderByDescending(p => p.Revenue)
            .Take(Math.Clamp(topProductCount, 1, 50))
            .ToList();

        var dailySales = revenueItems
            .GroupBy(i => i.Order.CreatedAt.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                Revenue = g.Sum(i => i.LineTotal),
                ItemsSold = g.Sum(i => i.Quantity),
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new SalesAnalyticsDto { TopProducts = topProducts, DailySales = dailySales };
    }

    public async Task<VisitorAnalyticsDto> GetVisitorAnalyticsAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetByProducerAsync(producerId, cancellationToken);

        return new VisitorAnalyticsDto
        {
            TotalViews = products.Sum(p => p.ViewCount),
            ProductViews = products
                .Select(p => new ProductViewDto { ProductId = p.Id, ProductName = p.Name, ViewCount = p.ViewCount })
                .OrderByDescending(p => p.ViewCount)
                .ToList(),
        };
    }

    public async Task<List<IncomeReportEntryDto>> GetIncomeReportAsync(
        Guid producerId, IncomeReportQueryParameters query, CancellationToken cancellationToken)
    {
        var items = await _producerOrderRepository.GetByProducerAsync(producerId, query.FromDate, query.ToDate, cancellationToken);
        var revenueItems = items.Where(i => RevenueStatuses.Contains(i.ProducerStatus)).ToList();

        return revenueItems
            .GroupBy(i => GetPeriodStart(i.Order.CreatedAt, query.GroupBy))
            .Select(g => new IncomeReportEntryDto
            {
                PeriodStart = g.Key,
                Revenue = g.Sum(i => i.LineTotal),
                OrderCount = g.Select(i => i.OrderId).Distinct().Count(),
                ItemsSold = g.Sum(i => i.Quantity),
            })
            .OrderBy(e => e.PeriodStart)
            .ToList();
    }

    public async Task<List<ProductPerformanceDto>> GetProductPerformanceAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetByProducerAsync(producerId, cancellationToken);
        var items = await _producerOrderRepository.GetByProducerAsync(producerId, null, null, cancellationToken);
        var revenueItems = items.Where(i => RevenueStatuses.Contains(i.ProducerStatus)).ToList();

        var revenueByProduct = revenueItems
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.LineTotal));

        return products
            .Select(p => new ProductPerformanceDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                ViewCount = p.ViewCount,
                SalesCount = p.SalesCount,
                Revenue = revenueByProduct.TryGetValue(p.Id, out var revenue) ? revenue : 0,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                ConversionRate = p.ViewCount == 0 ? 0 : Math.Round(p.SalesCount / (decimal)p.ViewCount, 4),
            })
            .OrderByDescending(p => p.Revenue)
            .ToList();
    }

    private async Task<OrderItem> GetOwnedItemAsync(Guid producerId, Guid orderItemId, CancellationToken cancellationToken)
    {
        var item = await _producerOrderRepository.GetByIdAsync(orderItemId, cancellationToken)
            ?? throw new NotFoundException("Order item not found.");

        if (item.Product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this order item.");
        }

        return item;
    }

    private static void EnsureStatus(OrderItem item, OrderItemProducerStatus required, string action)
    {
        if (item.ProducerStatus != required)
        {
            throw new ConflictException(
                $"Order item cannot be {action} from its current status ({item.ProducerStatus}).");
        }
    }

    private static DateTime GetPeriodStart(DateTime date, IncomeReportGroupBy groupBy)
    {
        date = date.Date;
        return groupBy switch
        {
            IncomeReportGroupBy.Week => date.AddDays(-(int)((7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7)),
            IncomeReportGroupBy.Month => new DateTime(date.Year, date.Month, 1),
            _ => date,
        };
    }

    private async Task<ProducerOrderItemDto> ToDtoWithCustomerAsync(OrderItem item, CancellationToken cancellationToken)
    {
        var customerInfo = await _producerOrderRepository.GetCustomerInfoAsync(new[] { item.Order.UserId }, cancellationToken);
        return ToDto(item, customerInfo);
    }

    private static ProducerOrderItemDto ToDto(OrderItem item, Dictionary<Guid, (string FullName, string Email)> customerInfo)
    {
        var customerName = customerInfo.TryGetValue(item.Order.UserId, out var info) ? info.FullName : "Unknown";

        return new ProducerOrderItemDto
        {
            Id = item.Id,
            OrderId = item.OrderId,
            OrderNumber = item.Order.OrderNumber,
            OrderStatus = item.Order.Status.ToString(),
            OrderCreatedAt = item.Order.CreatedAt,
            CustomerId = item.Order.UserId,
            CustomerName = customerName,
            RecipientName = item.Order.RecipientName,
            RecipientPhone = item.Order.RecipientPhone,
            ShippingAddressLine = item.Order.ShippingAddressLine,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ProductImageUrl = item.ProductImageUrl,
            ProductVariantId = item.ProductVariantId,
            VariantName = item.VariantName,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal,
            ProducerStatus = item.ProducerStatus.ToString(),
            ProducerNote = item.ProducerNote,
            ProducerRespondedAt = item.ProducerRespondedAt,
            TrackingNumber = item.TrackingNumber,
            Carrier = item.Carrier,
            ShippedAt = item.ShippedAt,
            DeliveredAt = item.DeliveredAt,
        };
    }
}

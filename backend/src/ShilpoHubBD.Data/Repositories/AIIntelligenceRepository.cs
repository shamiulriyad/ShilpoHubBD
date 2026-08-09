using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.AIIntelligence;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Procurement;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Data.Repositories;

public class AIIntelligenceRepository : IAIIntelligenceRepository
{
    private readonly ShilpoHubDbContext _context;

    public AIIntelligenceRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private class ProductAggregate
    {
        public int ProductCount { get; set; }
        public int HandmadeVerifiedCount { get; set; }
        public int TotalStock { get; set; }
        public int TotalReviewCount { get; set; }
        public decimal WeightedRatingSum { get; set; }
    }

    public async Task<ProducerIntelligenceProfileDto?> GetProducerIntelligenceProfileAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == producerId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var productAgg = await _context.Products
            .Where(p => p.ProducerId == producerId && p.IsActive)
            .GroupBy(p => p.ProducerId)
            .Select(g => new ProductAggregate
            {
                ProductCount = g.Count(),
                HandmadeVerifiedCount = g.Count(p => p.HandmadeVerificationStatus == HandmadeVerificationStatus.Verified),
                TotalStock = g.Sum(p => p.Stock),
                TotalReviewCount = g.Sum(p => p.ReviewCount),
                WeightedRatingSum = g.Sum(p => p.AverageRating * p.ReviewCount),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var orderItems = await _context.OrderItems
            .Where(oi => oi.Product.ProducerId == producerId)
            .Select(oi => new { oi.ProducerStatus, oi.DeliveredAt, OrderCreatedAt = oi.Order.CreatedAt })
            .ToListAsync(cancellationToken);

        var deliveredItems = orderItems
            .Where(i => i.ProducerStatus == OrderItemProducerStatus.Delivered && i.DeliveredAt.HasValue)
            .ToList();
        var cancelledCount = orderItems.Count(i => i.ProducerStatus == OrderItemProducerStatus.Cancelled);

        var heritageCertCount = await _context.HeritageCertifications
            .CountAsync(c => c.ProducerHeritageIdentity.ProducerId == producerId, cancellationToken);

        var sustainabilityCerts = await _context.SustainableMaterialCertifications
            .Where(c => c.SustainabilityProfile.ProducerId == producerId)
            .Select(c => c.IsVerified)
            .ToListAsync(cancellationToken);

        var quotationResponseStatuses = await _context.QuotationResponses
            .Where(r => r.QuotationRequestProducer.ProducerId == producerId)
            .Select(r => r.Status)
            .ToListAsync(cancellationToken);

        var procurementStatuses = await _context.ProcurementRequests
            .Where(p => p.ProducerId == producerId)
            .Select(p => p.Status)
            .ToListAsync(cancellationToken);

        return new ProducerIntelligenceProfileDto
        {
            ProducerId = producerId,
            ProducerName = user.FullName,
            AverageRating = productAgg is null || productAgg.TotalReviewCount == 0
                ? 0m
                : Math.Round(productAgg.WeightedRatingSum / productAgg.TotalReviewCount, 2),
            ReviewCount = productAgg?.TotalReviewCount ?? 0,
            ProductCount = productAgg?.ProductCount ?? 0,
            HandmadeVerifiedProductCount = productAgg?.HandmadeVerifiedCount ?? 0,
            EstimatedProductionCapacity = productAgg?.TotalStock ?? 0,
            CertificationCount = heritageCertCount + sustainabilityCerts.Count,
            HasVerifiedCertification = heritageCertCount > 0 || sustainabilityCerts.Any(v => v),
            TotalOrderItemCount = orderItems.Count,
            DeliveredOrderItemCount = deliveredItems.Count,
            CancelledOrderItemCount = cancelledCount,
            HistoricalDeliveryDays = deliveredItems.Select(i => (i.DeliveredAt!.Value - i.OrderCreatedAt).TotalDays).ToList(),
            TotalQuotationResponseCount = quotationResponseStatuses.Count,
            RejectedQuotationResponseCount = quotationResponseStatuses.Count(s => s == QuotationResponseStatus.Rejected),
            TotalProcurementCount = procurementStatuses.Count,
            CancelledProcurementCount = procurementStatuses.Count(s => s == ProcurementStatus.Cancelled),
        };
    }

    public async Task<List<PeriodPriceDto>> GetCategoryMonthlyAveragePriceAsync(Guid categoryId, int months, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var items = await _context.OrderItems
            .Where(oi => oi.Product.CategoryId == categoryId && oi.Order.CreatedAt >= cutoff)
            .Select(oi => new { oi.UnitPrice, OrderCreatedAt = oi.Order.CreatedAt })
            .ToListAsync(cancellationToken);

        return items
            .GroupBy(i => new DateTime(i.OrderCreatedAt.Year, i.OrderCreatedAt.Month, 1))
            .Select(g => new PeriodPriceDto { PeriodStart = g.Key, AveragePrice = Math.Round(g.Average(i => i.UnitPrice), 2) })
            .OrderBy(p => p.PeriodStart)
            .ToList();
    }
}

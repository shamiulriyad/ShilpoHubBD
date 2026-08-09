using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ProducerComparison;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerComparisonRepository : IProducerComparisonRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerComparisonRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private class ProductAggregate
    {
        public Guid ProducerId { get; set; }
        public int ProductCount { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public int TotalStock { get; set; }
        public int TotalReviewCount { get; set; }
        public decimal WeightedRatingSum { get; set; }
        public int TotalSalesCount { get; set; }
        public int HandmadeVerifiedCount { get; set; }
    }

    public async Task<List<ProducerComparisonRowDto>> CompareAsync(List<Guid> producerIds, CancellationToken cancellationToken)
    {
        var distinctIds = producerIds.Distinct().ToList();

        var validProducers = await _context.Users
            .Where(u => distinctIds.Contains(u.Id) && u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var validIds = validProducers.Keys.ToList();
        if (validIds.Count == 0)
        {
            return new List<ProducerComparisonRowDto>();
        }

        var productAggregates = await _context.Products
            .Where(p => p.IsActive && validIds.Contains(p.ProducerId))
            .GroupBy(p => p.ProducerId)
            .Select(g => new ProductAggregate
            {
                ProducerId = g.Key,
                ProductCount = g.Count(),
                MinPrice = g.Min(p => p.Price),
                MaxPrice = g.Max(p => p.Price),
                AveragePrice = g.Average(p => p.Price),
                TotalStock = g.Sum(p => p.Stock),
                TotalReviewCount = g.Sum(p => p.ReviewCount),
                WeightedRatingSum = g.Sum(p => p.AverageRating * p.ReviewCount),
                TotalSalesCount = g.Sum(p => p.SalesCount),
                HandmadeVerifiedCount = g.Count(p => p.HandmadeVerificationStatus == HandmadeVerificationStatus.Verified),
            })
            .ToDictionaryAsync(x => x.ProducerId, cancellationToken);

        var heritageIdentities = await _context.ProducerHeritageIdentities
            .Include(h => h.District)
            .Include(h => h.Certifications)
            .Where(h => validIds.Contains(h.ProducerId))
            .ToDictionaryAsync(h => h.ProducerId, cancellationToken);

        var sustainabilityProfiles = await _context.SustainabilityProfiles
            .Include(s => s.Certifications)
            .Where(s => validIds.Contains(s.ProducerId))
            .ToDictionaryAsync(s => s.ProducerId, cancellationToken);

        var deliveredItems = await _context.OrderItems
            .Where(oi => validIds.Contains(oi.Product.ProducerId)
                && oi.ProducerStatus == OrderItemProducerStatus.Delivered
                && oi.DeliveredAt.HasValue)
            .Select(oi => new { ProducerId = oi.Product.ProducerId, oi.OrderId, oi.Order.CreatedAt, DeliveredAt = oi.DeliveredAt!.Value })
            .ToListAsync(cancellationToken);

        var deliveryByProducer = deliveredItems
            .GroupBy(x => x.ProducerId)
            .ToDictionary(
                g => g.Key,
                g => (AverageDays: g.Average(x => (x.DeliveredAt - x.CreatedAt).TotalDays), OrderCount: g.Select(x => x.OrderId).Distinct().Count()));

        return distinctIds
            .Where(id => validProducers.ContainsKey(id))
            .Select(id =>
            {
                var user = validProducers[id];
                productAggregates.TryGetValue(id, out var agg);
                heritageIdentities.TryGetValue(id, out var heritage);
                sustainabilityProfiles.TryGetValue(id, out var sustainability);
                deliveryByProducer.TryGetValue(id, out var delivery);

                var certifications = new List<SupplierCertificationDto>();
                if (heritage is not null)
                {
                    certifications.AddRange(heritage.Certifications.Select(c => new SupplierCertificationDto
                    {
                        Source = "Heritage",
                        Name = c.Name,
                        IssuingBody = c.IssuingBody,
                        IsVerified = heritage.VerificationStatus == HeritageVerificationStatus.Verified,
                    }));
                }

                if (sustainability is not null)
                {
                    certifications.AddRange(sustainability.Certifications.Select(c => new SupplierCertificationDto
                    {
                        Source = "Sustainability",
                        Name = c.CertifyingBody,
                        IssuingBody = c.CertifyingBody,
                        IsVerified = c.IsVerified,
                    }));
                }

                var totalReviewCount = agg?.TotalReviewCount ?? 0;

                return new ProducerComparisonRowDto
                {
                    ProducerId = id,
                    ProducerName = user.FullName,
                    WorkshopName = heritage?.WorkshopName,
                    PrimaryCraft = heritage?.PrimaryCraft,
                    DistrictName = heritage?.District?.Name,
                    YearsOfExperience = heritage?.YearsOfExperience,
                    EstablishedYear = heritage?.EstablishedYear,
                    HeritageVerificationStatus = heritage?.VerificationStatus,
                    MinPrice = agg?.MinPrice,
                    MaxPrice = agg?.MaxPrice,
                    AveragePrice = agg is null ? null : Math.Round(agg.AveragePrice, 2),
                    AverageRating = totalReviewCount == 0 ? 0m : Math.Round(agg!.WeightedRatingSum / totalReviewCount, 2),
                    TotalReviewCount = totalReviewCount,
                    ProductCount = agg?.ProductCount ?? 0,
                    HandmadeVerifiedProductCount = agg?.HandmadeVerifiedCount ?? 0,
                    HandmadeVerifiedRatio = (agg is null || agg.ProductCount == 0) ? 0m : Math.Round((decimal)agg.HandmadeVerifiedCount / agg.ProductCount, 2),
                    EstimatedProductionCapacity = agg?.TotalStock ?? 0,
                    CertificationCount = certifications.Count,
                    Certifications = certifications,
                    AverageDeliveryDays = delivery.OrderCount == 0 ? null : Math.Round(delivery.AverageDays, 1),
                    TotalOrdersFulfilled = delivery.OrderCount,
                    TotalUnitsSold = agg?.TotalSalesCount ?? 0,
                };
            })
            .ToList();
    }
}

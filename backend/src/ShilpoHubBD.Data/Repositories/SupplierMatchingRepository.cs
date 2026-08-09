using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.SupplierMatching;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class SupplierMatchingRepository : ISupplierMatchingRepository
{
    private readonly ShilpoHubDbContext _context;

    public SupplierMatchingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private class CandidateRow
    {
        public Guid ProducerId { get; set; }
        public int ProductCount { get; set; }
        public decimal MinPrice { get; set; }
        public int TotalStock { get; set; }
        public int TotalReviewCount { get; set; }
        public decimal WeightedRatingSum { get; set; }
        public bool IsHandmadeVerified { get; set; }
        public bool HasMatchingCategory { get; set; }
        public bool HasMatchingDistrict { get; set; }
        public bool HasMatchingKeyword { get; set; }
        public bool HasProductWithinBudget { get; set; }
    }

    public async Task<List<SupplierMatchCandidateDto>> GetCandidatesAsync(SupplierMatchRequest request, CancellationToken cancellationToken)
    {
        // Boolean flags (rather than conditional/ternary expressions) are captured as plain parameters
        // so the whole aggregate query translates as a single SQL statement across every criterion,
        // including the ones the caller left unspecified.
        var hasCategory = request.CategoryId.HasValue;
        var categoryId = request.CategoryId ?? Guid.Empty;

        var hasDistrict = request.DistrictId.HasValue;
        var districtId = request.DistrictId ?? Guid.Empty;

        var hasKeyword = !string.IsNullOrWhiteSpace(request.ProductKeyword);
        var keywordTerm = hasKeyword ? $"%{request.ProductKeyword!.Trim()}%" : string.Empty;

        var hasBudget = request.MaxBudgetPerUnit.HasValue;
        var maxBudget = request.MaxBudgetPerUnit ?? 0m;

        var rows = await _context.Products
            .Where(p => p.IsActive)
            .GroupBy(p => p.ProducerId)
            .Select(g => new CandidateRow
            {
                ProducerId = g.Key,
                ProductCount = g.Count(),
                MinPrice = g.Min(p => p.Price),
                TotalStock = g.Sum(p => p.Stock),
                TotalReviewCount = g.Sum(p => p.ReviewCount),
                WeightedRatingSum = g.Sum(p => p.AverageRating * p.ReviewCount),
                IsHandmadeVerified = g.Any(p => p.HandmadeVerificationStatus == HandmadeVerificationStatus.Verified),
                HasMatchingCategory = hasCategory && g.Any(p => p.CategoryId == categoryId),
                HasMatchingDistrict = hasDistrict && g.Any(p => p.DistrictId == districtId),
                HasMatchingKeyword = hasKeyword && g.Any(p =>
                    EF.Functions.ILike(p.Name, keywordTerm) || EF.Functions.ILike(p.Description, keywordTerm)),
                HasProductWithinBudget = hasBudget && g.Any(p => p.Price <= maxBudget),
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return new List<SupplierMatchCandidateDto>();
        }

        var producerIds = rows.Select(r => r.ProducerId).ToList();

        var materialProducerIds = new HashSet<Guid>();
        if (!string.IsNullOrWhiteSpace(request.Material))
        {
            var materialTerm = $"%{request.Material.Trim()}%";
            var ids = await _context.SustainableMaterialRecords
                .Where(r => producerIds.Contains(r.SustainabilityProfile.ProducerId) && EF.Functions.ILike(r.MaterialName, materialTerm))
                .Select(r => r.SustainabilityProfile.ProducerId)
                .Distinct()
                .ToListAsync(cancellationToken);
            materialProducerIds = ids.ToHashSet();
        }

        var users = await _context.Users
            .Where(u => producerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var heritageIdentities = await _context.ProducerHeritageIdentities
            .Include(h => h.Certifications)
            .Include(h => h.District)
            .Where(h => producerIds.Contains(h.ProducerId))
            .ToDictionaryAsync(h => h.ProducerId, cancellationToken);

        var sustainabilityProfiles = await _context.SustainabilityProfiles
            .Include(s => s.Certifications)
            .Where(s => producerIds.Contains(s.ProducerId))
            .ToDictionaryAsync(s => s.ProducerId, cancellationToken);

        var deliveredItems = await _context.OrderItems
            .Where(oi => producerIds.Contains(oi.Product.ProducerId)
                && oi.ProducerStatus == OrderItemProducerStatus.Delivered
                && oi.DeliveredAt.HasValue)
            .Select(oi => new { ProducerId = oi.Product.ProducerId, oi.Order.CreatedAt, DeliveredAt = oi.DeliveredAt!.Value })
            .ToListAsync(cancellationToken);

        var averageDeliveryDaysByProducer = deliveredItems
            .GroupBy(x => x.ProducerId)
            .ToDictionary(g => g.Key, g => g.Average(x => (x.DeliveredAt - x.CreatedAt).TotalDays));

        return rows.Select(r =>
        {
            users.TryGetValue(r.ProducerId, out var user);
            heritageIdentities.TryGetValue(r.ProducerId, out var heritage);
            sustainabilityProfiles.TryGetValue(r.ProducerId, out var sustainability);
            averageDeliveryDaysByProducer.TryGetValue(r.ProducerId, out var averageDeliveryDays);

            var certificationCount = (heritage?.Certifications.Count ?? 0)
                + (sustainability?.Certifications.Count(c => c.IsVerified) ?? 0);

            return new SupplierMatchCandidateDto
            {
                ProducerId = r.ProducerId,
                ProducerName = user?.FullName ?? string.Empty,
                WorkshopName = heritage?.WorkshopName,
                PrimaryCraft = heritage?.PrimaryCraft,
                DistrictName = heritage?.District?.Name,
                ProductCount = r.ProductCount,
                MinPrice = r.MinPrice,
                EstimatedProductionCapacity = r.TotalStock,
                AverageRating = r.TotalReviewCount == 0 ? 0m : Math.Round(r.WeightedRatingSum / r.TotalReviewCount, 2),
                TotalReviewCount = r.TotalReviewCount,
                IsHandmadeVerified = r.IsHandmadeVerified,
                CertificationCount = certificationCount,
                AverageDeliveryDays = averageDeliveryDaysByProducer.ContainsKey(r.ProducerId) ? averageDeliveryDays : null,
                HasMatchingCategory = r.HasMatchingCategory,
                HasMatchingDistrict = r.HasMatchingDistrict,
                HasMatchingKeyword = r.HasMatchingKeyword,
                HasMatchingMaterial = materialProducerIds.Contains(r.ProducerId),
                HasProductWithinBudget = r.HasProductWithinBudget,
            };
        }).ToList();
    }
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.SupplierDiscovery;

namespace ShilpoHubBD.Data.Repositories;

public class SupplierDiscoveryRepository : ISupplierDiscoveryRepository
{
    private readonly ShilpoHubDbContext _context;

    public SupplierDiscoveryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private class ProducerAggregateRow
    {
        public Guid ProducerId { get; set; }
        public int ProductCount { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int TotalStock { get; set; }
        public int TotalReviewCount { get; set; }
        public decimal WeightedRatingSum { get; set; }
        public bool IsHandmadeVerified { get; set; }
        public DateTime LatestProductAt { get; set; }
    }

    public async Task<(List<SupplierSearchResultDto> Items, int TotalCount)> SearchAsync(
        SupplierSearchParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.Products.Where(p => p.IsActive);

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.DistrictId.HasValue)
        {
            query = query.Where(p => p.DistrictId == parameters.DistrictId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ProductName))
        {
            var term = $"%{parameters.ProductName.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = $"%{parameters.Search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, term) || EF.Functions.ILike(p.Producer.FullName, term));
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);
        }

        if (parameters.HandmadeVerifiedOnly == true)
        {
            query = query.Where(p => p.HandmadeVerificationStatus == HandmadeVerificationStatus.Verified);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Material) || parameters.CertifiedOnly == true)
        {
            var eligibleProducerIds = await GetProfileFilteredProducerIdsAsync(parameters, cancellationToken);
            query = eligibleProducerIds.Count == 0
                ? query.Where(_ => false)
                : query.Where(p => eligibleProducerIds.Contains(p.ProducerId));
        }

        var rows = await query
            .GroupBy(p => p.ProducerId)
            .Select(g => new ProducerAggregateRow
            {
                ProducerId = g.Key,
                ProductCount = g.Count(),
                MinPrice = g.Min(p => p.Price),
                MaxPrice = g.Max(p => p.Price),
                TotalStock = g.Sum(p => p.Stock),
                TotalReviewCount = g.Sum(p => p.ReviewCount),
                WeightedRatingSum = g.Sum(p => p.AverageRating * p.ReviewCount),
                IsHandmadeVerified = g.Any(p => p.HandmadeVerificationStatus == HandmadeVerificationStatus.Verified),
                LatestProductAt = g.Max(p => p.CreatedAt),
            })
            .ToListAsync(cancellationToken);

        // Rating is a ratio of two aggregates (guards divide-by-zero) and filtering/sorting on it is
        // done in-memory, over one row per matching producer, to avoid fragile conditional-division SQL.
        var enriched = rows
            .Select(r => (Row: r, AverageRating: r.TotalReviewCount == 0 ? 0m : Math.Round(r.WeightedRatingSum / r.TotalReviewCount, 2)))
            .AsEnumerable();

        if (parameters.MinRating.HasValue)
        {
            enriched = enriched.Where(x => x.AverageRating >= parameters.MinRating.Value);
        }

        if (parameters.MinProductionCapacity.HasValue)
        {
            enriched = enriched.Where(x => x.Row.TotalStock >= parameters.MinProductionCapacity.Value);
        }

        enriched = parameters.SortBy switch
        {
            SupplierSortOption.Newest => enriched.OrderByDescending(x => x.Row.LatestProductAt),
            SupplierSortOption.ProductCountDesc => enriched.OrderByDescending(x => x.Row.ProductCount),
            SupplierSortOption.PriceLowToHigh => enriched.OrderBy(x => x.Row.MinPrice),
            SupplierSortOption.PriceHighToLow => enriched.OrderByDescending(x => x.Row.MaxPrice),
            SupplierSortOption.ProductionCapacityDesc => enriched.OrderByDescending(x => x.Row.TotalStock),
            _ => enriched.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.Row.TotalReviewCount),
        };

        var enrichedList = enriched.ToList();
        var totalCount = enrichedList.Count;
        var pageItems = enrichedList
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var producerIds = pageItems.Select(x => x.Row.ProducerId).ToList();

        var users = await _context.Users
            .Where(u => producerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var heritageIdentities = await _context.ProducerHeritageIdentities
            .Include(h => h.District)
            .Include(h => h.Certifications)
            .Where(h => producerIds.Contains(h.ProducerId))
            .ToDictionaryAsync(h => h.ProducerId, cancellationToken);

        var sustainabilityProfiles = await _context.SustainabilityProfiles
            .Include(s => s.Certifications)
            .Where(s => producerIds.Contains(s.ProducerId))
            .ToDictionaryAsync(s => s.ProducerId, cancellationToken);

        var items = pageItems.Select(x =>
        {
            users.TryGetValue(x.Row.ProducerId, out var user);
            heritageIdentities.TryGetValue(x.Row.ProducerId, out var heritage);
            sustainabilityProfiles.TryGetValue(x.Row.ProducerId, out var sustainability);

            var certificationCount = (heritage?.Certifications.Count ?? 0)
                + (sustainability?.Certifications.Count(c => c.IsVerified) ?? 0);

            return new SupplierSearchResultDto
            {
                ProducerId = x.Row.ProducerId,
                ProducerName = user?.FullName ?? string.Empty,
                WorkshopName = heritage?.WorkshopName,
                PrimaryCraft = heritage?.PrimaryCraft,
                YearsOfExperience = heritage?.YearsOfExperience,
                DistrictId = heritage?.DistrictId,
                DistrictName = heritage?.District?.Name,
                HeritageVerificationStatus = heritage?.VerificationStatus,
                IsHandmadeVerified = x.Row.IsHandmadeVerified,
                AverageRating = x.AverageRating,
                TotalReviewCount = x.Row.TotalReviewCount,
                ProductCount = x.Row.ProductCount,
                MinPrice = x.Row.MinPrice,
                MaxPrice = x.Row.MaxPrice,
                EstimatedProductionCapacity = x.Row.TotalStock,
                CertificationCount = certificationCount,
                EcoScore = sustainability?.EcoScore,
                LegacyScore = heritage?.LegacyScore,
            };
        }).ToList();

        return (items, totalCount);
    }

    private async Task<HashSet<Guid>> GetProfileFilteredProducerIdsAsync(SupplierSearchParameters parameters, CancellationToken cancellationToken)
    {
        HashSet<Guid>? result = null;

        if (!string.IsNullOrWhiteSpace(parameters.Material))
        {
            var term = $"%{parameters.Material.Trim()}%";
            var materialProducerIds = await _context.SustainableMaterialRecords
                .Where(r => EF.Functions.ILike(r.MaterialName, term))
                .Select(r => r.SustainabilityProfile.ProducerId)
                .Distinct()
                .ToListAsync(cancellationToken);

            result = materialProducerIds.ToHashSet();
        }

        if (parameters.CertifiedOnly == true)
        {
            var heritageCertifiedIds = await _context.HeritageCertifications
                .Select(c => c.ProducerHeritageIdentity.ProducerId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var sustainabilityCertifiedIds = await _context.SustainableMaterialCertifications
                .Where(c => c.IsVerified)
                .Select(c => c.SustainabilityProfile.ProducerId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var certifiedIds = heritageCertifiedIds.Union(sustainabilityCertifiedIds).ToHashSet();

            result = result is null ? certifiedIds : result.Intersect(certifiedIds).ToHashSet();
        }

        return result ?? new HashSet<Guid>();
    }

    public async Task<SupplierProfileDto?> GetProducerProfileAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == producerId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var heritage = await _context.ProducerHeritageIdentities
            .Include(h => h.District)
            .Include(h => h.Certifications)
            .FirstOrDefaultAsync(h => h.ProducerId == producerId, cancellationToken);

        var sustainability = await _context.SustainabilityProfiles
            .Include(s => s.MaterialRecords)
            .Include(s => s.Certifications)
            .FirstOrDefaultAsync(s => s.ProducerId == producerId, cancellationToken);

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.ProducerId == producerId && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var totalReviewCount = products.Sum(p => p.ReviewCount);
        var weightedRatingSum = products.Sum(p => p.AverageRating * p.ReviewCount);

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

        return new SupplierProfileDto
        {
            ProducerId = user.Id,
            ProducerName = user.FullName,
            ProducerEmail = user.Email,
            MemberSince = user.CreatedAt,

            WorkshopName = heritage?.WorkshopName,
            WorkshopDescription = heritage?.WorkshopDescription,
            PrimaryCraft = heritage?.PrimaryCraft,
            YearsOfExperience = heritage?.YearsOfExperience,
            EstablishedYear = heritage?.EstablishedYear,
            DistrictName = heritage?.District?.Name,
            HeritageVerificationStatus = heritage?.VerificationStatus,
            LegacyScore = heritage?.LegacyScore,

            EcoScore = sustainability?.EcoScore,
            BadgeLevel = sustainability?.BadgeLevel,
            Materials = sustainability?.MaterialRecords.Select(m => m.MaterialName).Distinct().ToList() ?? new List<string>(),

            Certifications = certifications,

            ProductCount = products.Count,
            AverageRating = totalReviewCount == 0 ? 0m : Math.Round(weightedRatingSum / totalReviewCount, 2),
            TotalReviewCount = totalReviewCount,
            TotalSalesCount = products.Sum(p => p.SalesCount),
            MinPrice = products.Count == 0 ? null : products.Min(p => p.Price),
            MaxPrice = products.Count == 0 ? null : products.Max(p => p.Price),
            EstimatedProductionCapacity = products.Sum(p => p.Stock),

            Products = products.Select(p => new SupplierProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                ImageUrl = p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
                CategoryName = p.Category.Name,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                HandmadeVerificationStatus = p.HandmadeVerificationStatus,
            }).ToList(),
        };
    }
}

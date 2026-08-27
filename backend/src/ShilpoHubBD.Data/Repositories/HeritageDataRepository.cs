using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageDataRepository : IHeritageDataRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageDataRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<(List<HeritageLocationRecordDto> Items, int TotalCount)> GetLocationsAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        var places = _context.HeritagePlaces.AsQueryable();

        if (!query.IncludeInactive)
        {
            places = places.Where(p => p.IsActive);
        }

        if (query.DistrictId.HasValue)
        {
            places = places.Where(p => p.DistrictId == query.DistrictId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Division))
        {
            places = places.Where(p => p.District.Division == query.Division);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            places = places.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        places = places.OrderBy(p => p.Name);

        var totalCount = await places.CountAsync(cancellationToken);
        var items = await places
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new HeritageLocationRecordDto
            {
                Id = p.Id,
                Name = p.Name,
                PlaceType = p.PlaceType.ToString(),
                Address = p.Address,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                DistrictId = p.DistrictId,
                DistrictName = p.District.Name,
                Division = p.District.Division,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                IsActive = p.IsActive,
                UpdatedAt = p.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<HeritageVillageRecordDto> Items, int TotalCount)> GetVillagesAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        var villages = _context.Villages.AsQueryable();

        if (!query.IncludeInactive)
        {
            villages = villages.Where(v => v.IsActive);
        }

        if (query.DistrictId.HasValue)
        {
            villages = villages.Where(v => v.DistrictId == query.DistrictId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Division))
        {
            villages = villages.Where(v => v.District.Division == query.Division);
        }

        if (!string.IsNullOrWhiteSpace(query.Craft))
        {
            var craft = query.Craft.Trim().ToLower();
            villages = villages.Where(v => v.Craft.ToLower().Contains(craft));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            villages = villages.Where(v => v.Name.ToLower().Contains(term));
        }

        villages = villages.OrderBy(v => v.Name);

        var totalCount = await villages.CountAsync(cancellationToken);
        var items = await villages
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(v => new HeritageVillageRecordDto
            {
                Id = v.Id,
                Name = v.Name,
                Craft = v.Craft,
                Description = v.Description,
                DistrictId = v.DistrictId,
                DistrictName = v.District.Name,
                Division = v.District.Division,
                IsActive = v.IsActive,
                UpdatedAt = v.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<HeritageProducerRecordDto> Items, int TotalCount)> GetProducersAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        var rows =
            from user in _context.Users
            where user.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer)
            join phi in _context.ProducerHeritageIdentities on user.Id equals phi.ProducerId into phiGroup
            from identity in phiGroup.DefaultIfEmpty()
            select new { user, identity };

        if (!query.IncludeInactive)
        {
            rows = rows.Where(x => x.user.IsActive);
        }

        if (query.DistrictId.HasValue)
        {
            rows = rows.Where(x => x.identity != null && x.identity.DistrictId == query.DistrictId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Division))
        {
            rows = rows.Where(x => x.identity != null && x.identity.District != null
                && x.identity.District.Division == query.Division);
        }

        if (!string.IsNullOrWhiteSpace(query.Craft))
        {
            var craft = query.Craft.Trim().ToLower();
            rows = rows.Where(x => x.identity != null && x.identity.PrimaryCraft.ToLower().Contains(craft));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            rows = rows.Where(x => x.user.FullName.ToLower().Contains(term));
        }

        rows = rows.OrderBy(x => x.user.FullName);

        var totalCount = await rows.CountAsync(cancellationToken);
        var items = await rows
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new HeritageProducerRecordDto
            {
                ProducerId = x.user.Id,
                FullName = x.user.FullName,
                IsActive = x.user.IsActive,
                PrimaryCraft = x.identity != null ? x.identity.PrimaryCraft : null,
                YearsOfExperience = x.identity != null ? x.identity.YearsOfExperience : (int?)null,
                WorkshopName = x.identity != null ? x.identity.WorkshopName : null,
                EstablishedYear = x.identity != null ? x.identity.EstablishedYear : null,
                DistrictId = x.identity != null ? x.identity.DistrictId : null,
                DistrictName = x.identity != null && x.identity.District != null ? x.identity.District.Name : null,
                Division = x.identity != null && x.identity.District != null ? x.identity.District.Division : null,
                HeritageVerificationStatus = x.identity != null ? x.identity.VerificationStatus.ToString() : null,
                LegacyScore = x.identity != null ? x.identity.LegacyScore : (int?)null,
                ProductCount = _context.Products.Count(p => p.ProducerId == x.user.Id),
                JoinedAt = x.user.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<HeritageProductRecordDto> Items, int TotalCount)> GetProductsAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        var products = _context.Products.AsQueryable();

        if (!query.IncludeInactive)
        {
            products = products.Where(p => p.IsActive);
        }

        if (query.DistrictId.HasValue)
        {
            products = products.Where(p => p.DistrictId == query.DistrictId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Division))
        {
            products = products.Where(p => p.District.Division == query.Division);
        }

        if (!string.IsNullOrWhiteSpace(query.Craft))
        {
            var craft = query.Craft.Trim().ToLower();
            products = products.Where(p => p.Category.Name.ToLower().Contains(craft));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            products = products.Where(p => p.Name.ToLower().Contains(term));
        }

        products = products.OrderBy(p => p.Name);

        var totalCount = await products.CountAsync(cancellationToken);
        var items = await products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new HeritageProductRecordDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category.Name,
                ProducerId = p.ProducerId,
                ProducerName = p.Producer.FullName,
                DistrictId = p.DistrictId,
                DistrictName = p.District.Name,
                Division = p.District.Division,
                Price = p.Price,
                HandmadeVerificationStatus = p.HandmadeVerificationStatus.ToString(),
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                SalesCount = p.SalesCount,
                IsActive = p.IsActive,
                UpdatedAt = p.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<HeritageTourismRecordDto> Items, int TotalCount)> GetTourismAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        var services = _context.TouristServices.AsQueryable();

        if (!query.IncludeInactive)
        {
            services = services.Where(s => s.IsActive);
        }

        if (query.DistrictId.HasValue)
        {
            services = services.Where(s => s.DistrictId == query.DistrictId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Division))
        {
            services = services.Where(s => s.District.Division == query.Division);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            services = services.Where(s => s.Title.ToLower().Contains(term));
        }

        services = services.OrderBy(s => s.Title);

        var totalCount = await services.CountAsync(cancellationToken);
        var items = await services
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new HeritageTourismRecordDto
            {
                Id = s.Id,
                Title = s.Title,
                Type = s.Type.ToString(),
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                Location = s.Location,
                ProducerId = s.ProducerId,
                ProducerName = s.Producer.FullName,
                DistrictId = s.DistrictId,
                DistrictName = s.District.Name,
                Division = s.District.Division,
                AverageRating = s.AverageRating,
                ReviewCount = s.ReviewCount,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ProducerDemographicsDto> GetProducerDemographicsAsync(CancellationToken cancellationToken)
    {
        var totalProducers = await _context.Users
            .CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer), cancellationToken);

        var identities = await _context.ProducerHeritageIdentities
            .Where(i => i.Producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
            .Select(i => new
            {
                i.YearsOfExperience,
                i.PrimaryCraft,
                DistrictName = i.District != null ? i.District.Name : null,
                Division = i.District != null ? i.District.Division : null,
                Verification = i.VerificationStatus.ToString(),
            })
            .ToListAsync(cancellationToken);

        var withIdentity = identities.Count;
        var verified = identities.Count(i => i.Verification == "Verified");
        var avgExperience = identities.Count > 0
            ? Math.Round(identities.Average(i => (double)i.YearsOfExperience), 1)
            : 0;

        static HeritageCountBucketDto Bucket(string key, int count) => new() { Key = key, Label = key, Count = count };

        var byDivision = identities
            .Where(i => !string.IsNullOrWhiteSpace(i.Division))
            .GroupBy(i => i.Division!)
            .Select(g => Bucket(g.Key, g.Count()))
            .OrderByDescending(b => b.Count)
            .ToList();

        var byDistrict = identities
            .Where(i => !string.IsNullOrWhiteSpace(i.DistrictName))
            .GroupBy(i => i.DistrictName!)
            .Select(g => Bucket(g.Key, g.Count()))
            .OrderByDescending(b => b.Count)
            .Take(20)
            .ToList();

        var byCraft = identities
            .Where(i => !string.IsNullOrWhiteSpace(i.PrimaryCraft))
            .GroupBy(i => i.PrimaryCraft.Trim())
            .Select(g => Bucket(g.Key, g.Count()))
            .OrderByDescending(b => b.Count)
            .Take(20)
            .ToList();

        var byExperienceBand = identities
            .GroupBy(i => ExperienceBand(i.YearsOfExperience))
            .Select(g => Bucket(g.Key, g.Count()))
            .OrderBy(b => b.Key)
            .ToList();

        var byVerification = identities
            .GroupBy(i => i.Verification)
            .Select(g => Bucket(g.Key, g.Count()))
            .OrderByDescending(b => b.Count)
            .ToList();

        return new ProducerDemographicsDto
        {
            TotalProducers = totalProducers,
            WithHeritageIdentity = withIdentity,
            VerifiedHeritageIdentity = verified,
            AverageYearsOfExperience = avgExperience,
            ByDivision = byDivision,
            ByDistrict = byDistrict,
            ByPrimaryCraft = byCraft,
            ByExperienceBand = byExperienceBand,
            ByVerificationStatus = byVerification,
        };
    }

    public async Task<HeritageDatabaseSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var riskByLevel = await _context.HeritageRiskRecords
            .GroupBy(r => r.Level)
            .Select(g => new HeritageCountBucketDto
            {
                Key = g.Key.ToString(),
                Label = g.Key.ToString(),
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        var datasetsByCategory = await _context.HeritageDatasets
            .GroupBy(d => d.Category)
            .Select(g => new HeritageCountBucketDto
            {
                Key = g.Key.ToString(),
                Label = g.Key.ToString(),
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        return new HeritageDatabaseSummaryDto
        {
            Districts = await _context.Districts.CountAsync(cancellationToken),
            Villages = await _context.Villages.CountAsync(cancellationToken),
            HeritageLocations = await _context.HeritagePlaces.CountAsync(cancellationToken),
            Producers = await _context.Users.CountAsync(
                u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer), cancellationToken),
            Products = await _context.Products.CountAsync(cancellationToken),
            TourismServices = await _context.TouristServices.CountAsync(cancellationToken),
            RiskRecords = await _context.HeritageRiskRecords.CountAsync(cancellationToken),
            Datasets = await _context.HeritageDatasets.CountAsync(cancellationToken),
            PublishedDatasets = await _context.HeritageDatasets.CountAsync(
                d => d.Status == HeritageDatasetStatus.Published, cancellationToken),
            GeneratedAt = DateTime.UtcNow,
            RiskByLevel = riskByLevel,
            DatasetsByCategory = datasetsByCategory,
        };
    }

    public async Task<int> CountLiveRecordsAsync(HeritageDatasetCategory category, CancellationToken cancellationToken)
        => category switch
        {
            HeritageDatasetCategory.Locations => await _context.HeritagePlaces.CountAsync(p => p.IsActive, cancellationToken),
            HeritageDatasetCategory.Villages => await _context.Villages.CountAsync(v => v.IsActive, cancellationToken),
            HeritageDatasetCategory.Producers or HeritageDatasetCategory.Demographics => await _context.Users.CountAsync(
                u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer), cancellationToken),
            HeritageDatasetCategory.Products or HeritageDatasetCategory.Crafts => await _context.Products.CountAsync(
                p => p.IsActive, cancellationToken),
            HeritageDatasetCategory.Tourism => await _context.TouristServices.CountAsync(s => s.IsActive, cancellationToken),
            HeritageDatasetCategory.Risk => await _context.HeritageRiskRecords.CountAsync(cancellationToken),
            _ => 0,
        };

    private static string ExperienceBand(int years) => years switch
    {
        <= 2 => "0-2",
        <= 5 => "3-5",
        <= 10 => "6-10",
        <= 20 => "11-20",
        _ => "20+",
    };
}

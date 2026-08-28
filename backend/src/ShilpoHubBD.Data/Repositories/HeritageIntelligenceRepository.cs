using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageIntelligenceRepository : IHeritageIntelligenceRepository
{
    private static readonly OrderStatus[] CountedOrderStatuses =
    {
        OrderStatus.Pending, OrderStatus.Processing, OrderStatus.Shipped,
        OrderStatus.Delivered, OrderStatus.ReturnRequested,
    };

    private readonly ShilpoHubDbContext _context;

    public HeritageIntelligenceRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<GovScopeRef?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken)
        => await _context.Districts
            .Where(d => d.Id == districtId)
            .Select(d => new GovScopeRef(d.Id, d.Name, d.Division))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<GovVillageRef?> GetVillageAsync(Guid villageId, CancellationToken cancellationToken)
        => await _context.Villages
            .Where(v => v.Id == villageId)
            .Select(v => new GovVillageRef(v.Id, v.Name, v.Craft, v.DistrictId, v.District.Name))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<HeritageIntelligenceSignals> GatherSignalsAsync(
        HeritageIndexScope scope,
        Guid? scopeId,
        string? craftLabel,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        // Resolve the district a Village scope sits in, so area signals reuse the district filters.
        Guid? districtId = scope == HeritageIndexScope.District ? scopeId : null;
        string? craft = craftLabel?.Trim();
        if (scope == HeritageIndexScope.Village && scopeId.HasValue)
        {
            var village = await _context.Villages
                .Where(v => v.Id == scopeId.Value)
                .Select(v => new { v.DistrictId, v.Craft })
                .FirstOrDefaultAsync(cancellationToken);
            districtId = village?.DistrictId;
            craft ??= village?.Craft;
        }

        // ---- Producer id set in scope --------------------------------
        IQueryable<Guid> producerIds;
        if (districtId.HasValue)
        {
            producerIds = _context.Products
                .Where(p => p.DistrictId == districtId.Value)
                .Select(p => p.ProducerId)
                .Distinct();
        }
        else if (scope == HeritageIndexScope.Craft && !string.IsNullOrWhiteSpace(craft))
        {
            producerIds = _context.ProducerHeritageIdentities
                .Where(i => i.PrimaryCraft.ToLower() == craft!.ToLower())
                .Select(i => i.ProducerId)
                .Distinct();
        }
        else
        {
            producerIds = _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
                .Select(u => u.Id);
        }

        var producerIdList = await producerIds.ToListAsync(cancellationToken);

        var totalProducers = producerIdList.Count;
        var activeProducers = await _context.Users
            .CountAsync(u => producerIdList.Contains(u.Id) && u.IsActive, cancellationToken);

        var phi = _context.ProducerHeritageIdentities.AsQueryable();
        if (districtId.HasValue)
        {
            phi = phi.Where(i => i.DistrictId == districtId.Value);
        }
        else if (scope == HeritageIndexScope.Craft && !string.IsNullOrWhiteSpace(craft))
        {
            phi = phi.Where(i => i.PrimaryCraft.ToLower() == craft!.ToLower());
        }

        var verifiedHeritage = await phi
            .CountAsync(i => i.VerificationStatus == HeritageVerificationStatus.Verified, cancellationToken);

        var craftPractitioners = string.IsNullOrWhiteSpace(craft)
            ? verifiedHeritage
            : await _context.ProducerHeritageIdentities
                .CountAsync(i => i.PrimaryCraft.ToLower() == craft!.ToLower(), cancellationToken);

        // ---- Marketplace ------------------------------------------
        var products = _context.Products.Where(p => p.IsActive);
        var orderItems = _context.OrderItems
            .Where(i => CountedOrderStatuses.Contains(i.Order.Status))
            .Where(i => i.Order.CreatedAt >= from && i.Order.CreatedAt < to);

        if (districtId.HasValue)
        {
            products = products.Where(p => p.DistrictId == districtId.Value);
            orderItems = orderItems.Where(i => i.Product.DistrictId == districtId.Value);
        }
        else if (scope == HeritageIndexScope.Craft && !string.IsNullOrWhiteSpace(craft))
        {
            products = products.Where(p => producerIdList.Contains(p.ProducerId));
            orderItems = orderItems.Where(i => producerIdList.Contains(i.Product.ProducerId));
        }

        var activeProducts = await products.CountAsync(cancellationToken);
        var orders = await orderItems.Select(i => i.OrderId).Distinct().CountAsync(cancellationToken);
        var salesValue = await orderItems.Select(i => (decimal?)i.LineTotal).SumAsync(cancellationToken) ?? 0m;

        // ---- Community / culture ----------------------------------
        var villagesQuery = _context.Villages.AsQueryable();
        if (districtId.HasValue)
        {
            villagesQuery = villagesQuery.Where(v => v.DistrictId == districtId.Value);
        }
        else if (scope == HeritageIndexScope.Craft && !string.IsNullOrWhiteSpace(craft))
        {
            villagesQuery = villagesQuery.Where(v => v.Craft.ToLower() == craft!.ToLower());
        }

        if (scope == HeritageIndexScope.Village && scopeId.HasValue)
        {
            villagesQuery = _context.Villages.Where(v => v.Id == scopeId.Value);
        }

        var villages = await villagesQuery.CountAsync(cancellationToken);
        var activeVillages = await villagesQuery.CountAsync(v => v.IsActive, cancellationToken);

        var culturalEvents = districtId.HasValue
            ? await _context.CulturalEvents.CountAsync(e => e.DistrictId == districtId.Value, cancellationToken)
            : await _context.CulturalEvents.CountAsync(cancellationToken);
        var festivals = districtId.HasValue
            ? await _context.HeritageFestivals.CountAsync(f => f.DistrictId == districtId.Value, cancellationToken)
            : await _context.HeritageFestivals.CountAsync(cancellationToken);

        var storyEntries = await _context.StoryArchiveEntries
            .CountAsync(e => producerIdList.Contains(e.ProducerHeritageIdentity.ProducerId), cancellationToken);

        // ---- Risk assessments -----------------------------------
        var risk = _context.HeritageRiskRecords.AsQueryable();
        if (scope == HeritageIndexScope.Village && scopeId.HasValue)
        {
            risk = risk.Where(r => r.VillageId == scopeId.Value
                || (districtId.HasValue && r.DistrictId == districtId.Value));
        }
        else if (districtId.HasValue)
        {
            risk = risk.Where(r => r.DistrictId == districtId.Value);
        }
        else if (scope == HeritageIndexScope.Craft && !string.IsNullOrWhiteSpace(craft))
        {
            risk = risk.Where(r => r.CraftName != null && r.CraftName.ToLower() == craft!.ToLower());
        }

        var riskLow = await risk.CountAsync(r => r.Level == HeritageRiskLevel.Low, cancellationToken);
        var riskModerate = await risk.CountAsync(r => r.Level == HeritageRiskLevel.Moderate, cancellationToken);
        var riskHigh = await risk.CountAsync(r => r.Level == HeritageRiskLevel.High, cancellationToken);
        var riskCritical = await risk.CountAsync(r => r.Level == HeritageRiskLevel.Critical, cancellationToken);
        var riskSafeguarded = await risk.CountAsync(r => r.Level == HeritageRiskLevel.Safeguarded, cancellationToken);
        var climateRecords = await risk.CountAsync(r => r.Category == HeritageRiskCategory.ClimateThreat, cancellationToken);
        var materialRecords = await risk.CountAsync(r => r.Category == HeritageRiskCategory.RawMaterialScarcity, cancellationToken);
        var affectedArtisans = await risk
            .Select(r => (int?)(r.AffectedArtisanCount ?? 0))
            .SumAsync(cancellationToken) ?? 0;

        // ---- Youth / learning pipeline -----------------------------
        // Apprenticeship / academy records carry no reliable district link, so these are
        // platform-wide counts within the window regardless of scope (documented in the service).
        var apprenticeEnrollments = await _context.ApprenticeEnrollments
            .CountAsync(e => e.EnrolledAt >= from && e.EnrolledAt < to, cancellationToken);
        var programApplications = await _context.ProgramApplications
            .CountAsync(a => a.AppliedAt >= from && a.AppliedAt < to, cancellationToken);
        var courseEnrollments = await _context.CourseEnrollments
            .CountAsync(e => e.EnrolledAt >= from && e.EnrolledAt < to, cancellationToken);
        var academyLearners = await _context.AcademyMemberProfiles
            .CountAsync(p => p.Role == AcademyMemberRole.Learner, cancellationToken);
        var mentorshipRequests = await _context.MentorshipRequests
            .CountAsync(r => r.RequestedAt >= from && r.RequestedAt < to, cancellationToken);

        return new HeritageIntelligenceSignals
        {
            TotalProducers = totalProducers,
            ActiveProducers = activeProducers,
            VerifiedHeritageProducers = verifiedHeritage,
            CraftPractitioners = craftPractitioners,
            ActiveProducts = activeProducts,
            Orders = orders,
            SalesValue = salesValue,
            Villages = villages,
            ActiveVillages = activeVillages,
            CulturalEvents = culturalEvents,
            HeritageFestivals = festivals,
            StoryArchiveEntries = storyEntries,
            RiskLow = riskLow,
            RiskModerate = riskModerate,
            RiskHigh = riskHigh,
            RiskCritical = riskCritical,
            RiskSafeguarded = riskSafeguarded,
            ClimateRiskRecords = climateRecords,
            MaterialScarcityRecords = materialRecords,
            AffectedArtisans = affectedArtisans,
            ApprenticeEnrollments = apprenticeEnrollments,
            ProgramApplications = programApplications,
            AcademyLearners = academyLearners,
            MentorshipRequests = mentorshipRequests,
            CourseEnrollments = courseEnrollments,
        };
    }

    // ---- Persistence ----------------------------------------------

    public async Task AddAsync(HeritageIndexRecord record, CancellationToken cancellationToken)
        => await _context.HeritageIndexRecords.AddAsync(record, cancellationToken);

    public void Remove(HeritageIndexRecord record)
        => _context.HeritageIndexRecords.Remove(record);

    public Task<HeritageIndexRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.HeritageIndexRecords
            .Include(r => r.GeneratedBy)
            .Include(r => r.Components)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(List<HeritageIndexRecord> Items, int TotalCount)> GetPagedAsync(
        HeritageIndexQueryParameters query, CancellationToken cancellationToken)
    {
        var records = _context.HeritageIndexRecords
            .Include(r => r.GeneratedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.IndexType)
            && Enum.TryParse<HeritageIndexType>(query.IndexType, true, out var indexType))
        {
            records = records.Where(r => r.IndexType == indexType);
        }

        if (!string.IsNullOrWhiteSpace(query.Scope)
            && Enum.TryParse<HeritageIndexScope>(query.Scope, true, out var scope))
        {
            records = records.Where(r => r.Scope == scope);
        }

        if (query.ScopeId.HasValue)
        {
            records = records.Where(r => r.ScopeId == query.ScopeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.CraftLabel))
        {
            var term = query.CraftLabel.Trim().ToLower();
            records = records.Where(r => r.ScopeLabel.ToLower() == term);
        }

        records = records.OrderByDescending(r => r.ComputedAt);

        var totalCount = await records.CountAsync(cancellationToken);
        var items = await records
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<HeritageIndexRecord>> GetForTrendAsync(
        HeritageIndexType indexType,
        HeritageIndexScope scope,
        Guid? scopeId,
        string? craftLabel,
        int take,
        CancellationToken cancellationToken)
    {
        var records = _context.HeritageIndexRecords
            .Where(r => r.IndexType == indexType && r.Scope == scope);

        if (scopeId.HasValue)
        {
            records = records.Where(r => r.ScopeId == scopeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(craftLabel))
        {
            var term = craftLabel.Trim().ToLower();
            records = records.Where(r => r.ScopeLabel.ToLower() == term);
        }

        return records
            .OrderByDescending(r => r.PeriodEnd)
            .ThenByDescending(r => r.ComputedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

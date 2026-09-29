using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProducerBusiness;

namespace ShilpoHubBD.Application.Services.ProducerBusiness;

public class ProducerMonthlyReportService : IProducerMonthlyReportService
{
    private readonly IProducerMonthlyReportRepository _reportRepository;
    private readonly IProducerOrderRepository _producerOrderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReviewRepository _reviewRepository;
    // Only used for its producer-scoped refund query (GetRefundDeductionsAsync) — that logic isn't
    // specific to partnerships, it just hasn't had a home outside that repository until now.
    private readonly IProducerPartnershipSettlementRepository _settlementRepository;
    // Only used for its latest-case-per-producer lookup, to annotate the intelligence list with a
    // Government/NGO support status — never used to compute or mutate case data here.
    private readonly IArtisanSupportRepository _artisanSupportRepository;

    public ProducerMonthlyReportService(
        IProducerMonthlyReportRepository reportRepository, IProducerOrderRepository producerOrderRepository,
        IProductRepository productRepository, IReviewRepository reviewRepository,
        IProducerPartnershipSettlementRepository settlementRepository, IArtisanSupportRepository artisanSupportRepository)
    {
        _reportRepository = reportRepository;
        _producerOrderRepository = producerOrderRepository;
        _productRepository = productRepository;
        _reviewRepository = reviewRepository;
        _settlementRepository = settlementRepository;
        _artisanSupportRepository = artisanSupportRepository;
    }

    public async Task<ProducerMonthlyReportGenerationResultDto> GenerateForMonthAsync(
        int year, int month, CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12)
        {
            throw new ConflictException("Month must be between 1 and 12.");
        }

        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1).AddTicks(-1);

        if (periodEnd > DateTime.UtcNow)
        {
            throw new ConflictException("Cannot generate a report for a month that hasn't ended yet.");
        }

        var producerIds = await _reportRepository.GetProducerIdsAsync(cancellationToken);

        var newReports = new List<ProducerMonthlyReport>();
        var skipped = 0;

        foreach (var producerId in producerIds)
        {
            if (await _reportRepository.ExistsAsync(producerId, year, month, cancellationToken))
            {
                skipped++;
                continue;
            }

            var report = await BuildReportAsync(producerId, year, month, periodStart, periodEnd, cancellationToken);
            await _reportRepository.AddAsync(report, cancellationToken);
            newReports.Add(report);
        }

        if (newReports.Count > 0)
        {
            // Persist the base metrics first, then rank against the full cohort for this month —
            // including reports a previous run already generated, which are read back here but never
            // written to (only rows in newReports get their positioning fields set below).
            await _reportRepository.SaveChangesAsync(cancellationToken);

            var allReportsThisMonth = await _reportRepository.GetAllForMonthAsync(year, month, cancellationToken);
            ApplyPositioning(newReports, allReportsThisMonth);

            await _reportRepository.SaveChangesAsync(cancellationToken);
        }

        return new ProducerMonthlyReportGenerationResultDto
        {
            Year = year,
            Month = month,
            ProducerCount = producerIds.Count,
            GeneratedCount = newReports.Count,
            SkippedCount = skipped,
        };
    }

    public async Task<PagedResult<ProducerMonthlyReportDto>> GetPagedAsync(
        ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _reportRepository.GetPagedAsync(query, cancellationToken);

        return new PagedResult<ProducerMonthlyReportDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ProducerMonthlyReportDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Producer monthly report not found.");
        return ToDto(report);
    }

    public async Task<ProducerMonthlyReportDto> GetForProducerAsync(
        Guid producerId, int year, int month, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetForProducerWithDetailsAsync(producerId, year, month, cancellationToken)
            ?? throw new NotFoundException("This producer has no report for that month.");
        return ToDto(report);
    }

    public async Task<ProducerMonthlyReportComparisonDto> CompareAsync(
        Guid producerId, int? year, int? month, CancellationToken cancellationToken)
    {
        ProducerMonthlyReport current;
        if (year.HasValue && month.HasValue)
        {
            current = await _reportRepository.GetForProducerWithDetailsAsync(producerId, year.Value, month.Value, cancellationToken)
                ?? throw new NotFoundException("This producer has no report for that month.");
        }
        else
        {
            current = await _reportRepository.GetLatestForProducerAsync(producerId, cancellationToken)
                ?? throw new NotFoundException("This producer has no reports yet.");
        }

        var previousMonthDate = new DateTime(current.Year, current.Month, 1).AddMonths(-1);
        var previous = await _reportRepository.GetForProducerWithDetailsAsync(
            producerId, previousMonthDate.Year, previousMonthDate.Month, cancellationToken);

        var comparison = new ProducerMonthlyReportComparisonDto
        {
            CurrentMonth = ToDto(current),
            PreviousMonth = previous is null ? null : ToDto(previous),
        };

        if (previous is not null)
        {
            comparison.SalesChange = current.TotalSales - previous.TotalSales;
            comparison.OrdersChange = current.TotalOrders - previous.TotalOrders;
            comparison.NetIncomeChange = current.NetIncome - previous.NetIncome;
            comparison.UnitsSoldChange = current.UnitsSold - previous.UnitsSold;
            comparison.CancellationRateChange = current.CancellationRate - previous.CancellationRate;
            comparison.OverallSalesRankChange = current.OverallSalesRank - previous.OverallSalesRank;
            comparison.AverageRatingChange = current.AverageRating.HasValue && previous.AverageRating.HasValue
                ? current.AverageRating.Value - previous.AverageRating.Value
                : null;
        }

        return comparison;
    }

    public async Task<ProducerMonthlyReportShareResultDto> ShareAsync(
        Guid sharedByUserId, ShareProducerMonthlyReportsRequest request, CancellationToken cancellationToken)
    {
        var reportIds = request.ReportIds.Distinct().ToList();
        var sharedWithUserIds = request.SharedWithUserIds.Distinct().ToList();

        foreach (var reportId in reportIds)
        {
            if (!await _reportRepository.ReportExistsAsync(reportId, cancellationToken))
            {
                throw new NotFoundException($"Report {reportId} not found.");
            }
        }

        var validGovernmentUserIds = await _reportRepository.FilterByRoleAsync(sharedWithUserIds, RoleNames.GovernmentNGO, cancellationToken);
        var invalidUserIds = sharedWithUserIds.Except(validGovernmentUserIds).ToList();
        if (invalidUserIds.Count > 0)
        {
            throw new ConflictException(
                $"The following users are not Government/NGO accounts: {string.Join(", ", invalidUserIds)}.");
        }

        var now = DateTime.UtcNow;
        var sharedCount = 0;
        var alreadySharedCount = 0;

        foreach (var reportId in reportIds)
        {
            var alreadySharedWith = await _reportRepository.GetExistingShareUserIdsAsync(reportId, sharedWithUserIds, cancellationToken);

            foreach (var userId in sharedWithUserIds)
            {
                if (alreadySharedWith.Contains(userId))
                {
                    alreadySharedCount++;
                    continue;
                }

                await _reportRepository.AddShareAsync(new ProducerMonthlyReportShare
                {
                    Id = Guid.NewGuid(),
                    ReportId = reportId,
                    SharedWithUserId = userId,
                    SharedByUserId = sharedByUserId,
                    SharedAt = now,
                }, cancellationToken);
                sharedCount++;
            }
        }

        await _reportRepository.SaveChangesAsync(cancellationToken);

        return new ProducerMonthlyReportShareResultDto
        {
            SharedCount = sharedCount,
            AlreadySharedCount = alreadySharedCount,
        };
    }

    public async Task<List<ProducerMonthlyReportShareDto>> GetSharesForReportAsync(Guid reportId, CancellationToken cancellationToken)
    {
        var shares = await _reportRepository.GetSharesForReportAsync(reportId, cancellationToken);
        return shares.Select(s => new ProducerMonthlyReportShareDto
        {
            Id = s.Id,
            ReportId = s.ReportId,
            SharedWithUserId = s.SharedWithUserId,
            SharedWithName = s.SharedWithUser.FullName,
            SharedWithEmail = s.SharedWithUser.Email,
            SharedByUserId = s.SharedByUserId,
            SharedByName = s.SharedByUser.FullName,
            SharedAt = s.SharedAt,
        }).ToList();
    }

    public async Task<ProducerMonthlyReportDto> GetSharedReportAsync(Guid id, Guid governmentUserId, CancellationToken cancellationToken)
    {
        if (!await _reportRepository.IsSharedWithAsync(id, governmentUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException("This report has not been shared with you.");
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PagedResult<ProducerIntelligenceRowDto>> GetIntelligenceListAsync(
        ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _reportRepository.GetPagedAsync(query, cancellationToken);

        var supportStatuses = await _artisanSupportRepository.GetLatestCasesForArtisansAsync(
            items.Select(r => r.ProducerId), cancellationToken);

        return new PagedResult<ProducerIntelligenceRowDto>
        {
            Items = items.Select(r => ToIntelligenceRowDto(r, supportStatuses)).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ProducerIntelligenceDashboardDto> GetIntelligenceDashboardAsync(
        ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
    {
        var reports = await _reportRepository.GetAllMatchingAsync(query, cancellationToken);
        var rated = reports.Where(r => r.AverageRating.HasValue).ToList();

        return new ProducerIntelligenceDashboardDto
        {
            Year = query.Year,
            Month = query.Month,
            TotalProducers = reports.Count,
            MonthlySales = reports.Sum(r => r.TotalSales),
            AverageProducerIncome = reports.Count == 0 ? 0 : Math.Round(reports.Average(r => r.NetIncome), 2),
            AverageRating = rated.Count == 0 ? null : Math.Round(rated.Average(r => r.AverageRating!.Value), 2),
            GrowingProducers = reports.Count(r => r.SalesGrowthPercentage is > 0),
            DecliningProducers = reports.Count(r => r.SalesGrowthPercentage is < 0),
            ProducersNeedingSupport = reports.Count(r => DetectProblems(r).Count > 0),
        };
    }

    /// <summary>
    /// Rule-based, deterministic flags computed directly from this report's own already-computed
    /// numbers — no AI, no new calculation. Thresholds mirror the same style already used for
    /// DummyProductIntelligenceAIProvider's rating check (below 3.5) and the significant-change
    /// convention used elsewhere in this project (a 10%+ move).
    /// </summary>
    private static List<string> DetectProblems(ProducerMonthlyReport report)
    {
        var problems = new List<string>();

        if (report.AverageRating.HasValue && report.AverageRating.Value < 3.5m)
        {
            problems.Add("Low average rating");
        }

        if (report.CancellationRate > 20m)
        {
            problems.Add("High cancellation rate");
        }

        if (report.SalesGrowthPercentage is <= -10m)
        {
            problems.Add("Sales declining");
        }

        if (report.TotalOrders == 0)
        {
            problems.Add("No orders this month");
        }

        return problems;
    }

    private static ProducerIntelligenceRowDto ToIntelligenceRowDto(
        ProducerMonthlyReport report, Dictionary<Guid, Domain.Entities.Governance.ArtisanSupportCase> supportStatuses) => new()
    {
        ReportId = report.Id,
        ProducerId = report.ProducerId,
        ProducerName = report.Producer.FullName,
        ProducerEmail = report.Producer.Email,
        Year = report.Year,
        Month = report.Month,
        Sales = report.TotalSales,
        Income = report.NetIncome,
        Orders = report.TotalOrders,
        Rating = report.AverageRating,
        Growth = report.SalesGrowthPercentage,
        Position = report.OverallSalesRank,
        PositionPercentile = report.OverallSalesPercentile,
        DetectedProblems = DetectProblems(report),
        SupportStatus = supportStatuses.TryGetValue(report.ProducerId, out var latestCase)
            ? latestCase.Status.ToString()
            : "No active support case",
    };

    private static ProducerMonthlyReportDto ToDto(ProducerMonthlyReport report) => new()
    {
        Id = report.Id,
        ProducerId = report.ProducerId,
        ProducerName = report.Producer.FullName,
        ProducerEmail = report.Producer.Email,
        Year = report.Year,
        Month = report.Month,
        TotalOrders = report.TotalOrders,
        TotalSales = report.TotalSales,
        NetIncome = report.NetIncome,
        AverageRating = report.AverageRating,
        ReviewCount = report.ReviewCount,
        CancelledOrders = report.CancelledOrders,
        CancellationRate = report.CancellationRate,
        ProductCount = report.ProductCount,
        UnitsSold = report.UnitsSold,
        NewCustomers = report.NewCustomers,
        ReturningCustomers = report.ReturningCustomers,
        PreviousMonthSales = report.PreviousMonthSales,
        SalesGrowthPercentage = report.SalesGrowthPercentage,
        OverallSalesRank = report.OverallSalesRank,
        OverallSalesPercentile = report.OverallSalesPercentile,
        CategoryId = report.CategoryId,
        CategoryName = report.Category?.Name,
        CategoryPosition = report.CategoryPosition,
        CategoryAverageSales = report.CategoryAverageSales,
        DistrictId = report.DistrictId,
        DistrictName = report.District?.Name,
        DistrictPosition = report.DistrictPosition,
        DistrictAverageSales = report.DistrictAverageSales,
        PeerAverageGrowthPercentage = report.PeerAverageGrowthPercentage,
        DetectedProblems = DetectProblems(report),
        CreatedAt = report.CreatedAt,
    };

    private async Task<ProducerMonthlyReport> BuildReportAsync(
        Guid producerId, int year, int month, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken)
    {
        // Everything up to and including this month, split in-memory into "this month" vs "before this
        // month" — one fetch instead of two, and the "before" half is what tells new from returning
        // customers apart. Cost grows with the producer's full order history; acceptable for a batch
        // job that runs monthly, matching the unpaged-fetch tradeoff IProducerOrderRepository already accepts elsewhere.
        var itemsUpToPeriodEnd = await _producerOrderRepository.GetByProducerAsync(producerId, null, periodEnd, cancellationToken);
        var periodItems = itemsUpToPeriodEnd.Where(i => i.Order.CreatedAt >= periodStart).ToList();
        var priorItems = itemsUpToPeriodEnd.Where(i => i.Order.CreatedAt < periodStart).ToList();

        // Mirrors ProducerOrderService's revenue rule: an item only counts as sold once delivered.
        var deliveredPeriodItems = periodItems.Where(i => i.ProducerStatus == OrderItemProducerStatus.Delivered).ToList();
        var totalSales = deliveredPeriodItems.Sum(i => i.LineTotal);
        var unitsSold = deliveredPeriodItems.Sum(i => i.Quantity);

        var totalOrders = periodItems.Select(i => i.OrderId).Distinct().Count();
        var cancelledOrders = periodItems
            .Where(i => i.ProducerStatus == OrderItemProducerStatus.Cancelled)
            .Select(i => i.OrderId).Distinct().Count();
        var cancellationRate = totalOrders == 0 ? 0m : Math.Round(cancelledOrders * 100m / totalOrders, 2);

        var refundDeductions = await _settlementRepository.GetRefundDeductionsAsync(producerId, periodStart, periodEnd, cancellationToken);
        var netIncome = Math.Max(0, totalSales - refundDeductions);

        var periodBuyers = periodItems.Select(i => i.Order.UserId).Distinct().ToHashSet();
        var priorBuyers = priorItems.Select(i => i.Order.UserId).Distinct().ToHashSet();
        var newCustomers = periodBuyers.Count(b => !priorBuyers.Contains(b));
        var returningCustomers = periodBuyers.Count(b => priorBuyers.Contains(b));

        // "Listed" products only — the same ApprovalStatus.Approved gate the public storefront uses.
        var products = await _productRepository.GetByProducerAsync(producerId, cancellationToken);
        var productCount = products.Count(p => p.ApprovalStatus != ProductApprovalStatus.Rejected && p.CreatedAt <= periodEnd);

        var (averageRating, reviewCount) = await _reviewRepository.GetAggregateByProducerAsync(producerId, periodStart, periodEnd, cancellationToken);

        // The category/district that generated the most of this producer's delivered revenue this
        // month — the basis for CategoryPosition/DistrictPosition below. No dominant category/district
        // when there were no delivered sales this month (ties broken by Id for determinism).
        var dominantCategoryId = deliveredPeriodItems
            .GroupBy(i => i.Product.CategoryId)
            .Select(g => new { CategoryId = g.Key, Sales = g.Sum(i => i.LineTotal) })
            .OrderByDescending(g => g.Sales).ThenBy(g => g.CategoryId)
            .Select(g => (Guid?)g.CategoryId)
            .FirstOrDefault();

        var dominantDistrictId = deliveredPeriodItems
            .GroupBy(i => i.Product.DistrictId)
            .Select(g => new { DistrictId = g.Key, Sales = g.Sum(i => i.LineTotal) })
            .OrderByDescending(g => g.Sales).ThenBy(g => g.DistrictId)
            .Select(g => (Guid?)g.DistrictId)
            .FirstOrDefault();

        var previousMonthDate = periodStart.AddMonths(-1);
        var previousReport = await _reportRepository.GetAsync(producerId, previousMonthDate.Year, previousMonthDate.Month, cancellationToken);
        var previousMonthSales = previousReport?.TotalSales ?? 0m;
        decimal? salesGrowthPercentage = previousMonthSales == 0m
            ? null
            : Math.Round((totalSales - previousMonthSales) * 100m / previousMonthSales, 2);

        return new ProducerMonthlyReport
        {
            Id = Guid.NewGuid(),
            ProducerId = producerId,
            Month = month,
            Year = year,
            TotalOrders = totalOrders,
            TotalSales = totalSales,
            NetIncome = netIncome,
            AverageRating = reviewCount == 0 ? null : (decimal)averageRating,
            ReviewCount = reviewCount,
            CancelledOrders = cancelledOrders,
            CancellationRate = cancellationRate,
            ProductCount = productCount,
            UnitsSold = unitsSold,
            NewCustomers = newCustomers,
            ReturningCustomers = returningCustomers,
            PreviousMonthSales = previousMonthSales,
            SalesGrowthPercentage = salesGrowthPercentage,
            CategoryId = dominantCategoryId,
            DistrictId = dominantDistrictId,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Cross-sectional positioning for this run's newly-created reports, computed against every report
    /// that now exists for this month (including ones a prior run already generated and froze).
    /// Every position here is a plain ordinal rank on TotalSales — highest sales first, ties broken by
    /// ProducerId for full determinism — the same technique already used for district leaderboards in
    /// NationalDashboardService.GetDistrictRankingsAsync. No weighted or composite score is introduced.
    /// </summary>
    private static void ApplyPositioning(List<ProducerMonthlyReport> newReports, List<ProducerMonthlyReport> allReportsThisMonth)
    {
        var totalCount = allReportsThisMonth.Count;

        var overallRankByProducer = allReportsThisMonth
            .OrderByDescending(r => r.TotalSales).ThenBy(r => r.ProducerId)
            .Select((r, index) => (r.ProducerId, Rank: index + 1))
            .ToDictionary(x => x.ProducerId, x => x.Rank);

        var byCategory = allReportsThisMonth
            .Where(r => r.CategoryId.HasValue)
            .GroupBy(r => r.CategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.TotalSales).ThenBy(r => r.ProducerId).ToList());

        var byDistrict = allReportsThisMonth
            .Where(r => r.DistrictId.HasValue)
            .GroupBy(r => r.DistrictId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.TotalSales).ThenBy(r => r.ProducerId).ToList());

        var growthByProducer = allReportsThisMonth
            .Where(r => r.SalesGrowthPercentage.HasValue)
            .ToDictionary(r => r.ProducerId, r => r.SalesGrowthPercentage!.Value);
        var growthSum = growthByProducer.Values.Sum();
        var growthCount = growthByProducer.Count;

        foreach (var report in newReports)
        {
            report.OverallSalesRank = overallRankByProducer[report.ProducerId];
            // Percentile = share of the peer group this producer outsold; undefined with only one producer.
            report.OverallSalesPercentile = totalCount > 1
                ? Math.Round((totalCount - report.OverallSalesRank) * 100m / (totalCount - 1), 2)
                : null;

            if (report.CategoryId.HasValue && byCategory.TryGetValue(report.CategoryId.Value, out var categoryPeers))
            {
                report.CategoryPosition = categoryPeers.FindIndex(r => r.ProducerId == report.ProducerId) + 1;
                report.CategoryAverageSales = Math.Round(categoryPeers.Average(r => r.TotalSales), 2);
            }

            if (report.DistrictId.HasValue && byDistrict.TryGetValue(report.DistrictId.Value, out var districtPeers))
            {
                report.DistrictPosition = districtPeers.FindIndex(r => r.ProducerId == report.ProducerId) + 1;
                report.DistrictAverageSales = Math.Round(districtPeers.Average(r => r.TotalSales), 2);
            }

            // Average growth of every OTHER producer — this report's own figure is excluded from both
            // the sum and the count before averaging.
            var hasOwnGrowth = growthByProducer.TryGetValue(report.ProducerId, out var ownGrowth);
            var peerGrowthSum = hasOwnGrowth ? growthSum - ownGrowth : growthSum;
            var peerGrowthCount = hasOwnGrowth ? growthCount - 1 : growthCount;
            report.PeerAverageGrowthPercentage = peerGrowthCount > 0
                ? Math.Round(peerGrowthSum / peerGrowthCount, 2)
                : null;
        }
    }
}

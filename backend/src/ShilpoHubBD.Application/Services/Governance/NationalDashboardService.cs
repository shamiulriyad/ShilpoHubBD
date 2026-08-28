using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO National Dashboard. All figures are aggregated live from the existing
/// marketplace / employment / tourism / community tables; snapshots persist a period's numbers so
/// trends can be charted over time.
/// </summary>
public class NationalDashboardService : INationalDashboardService
{
    private const int MaxRankingRows = 64;
    private const int MaxTrendPoints = 60;

    private static readonly string[] RankingMetrics =
        { "sales", "producers", "products", "villages", "orders" };

    private static readonly string[] TrendMetrics =
    {
        "producers", "activeproducers", "newproducers", "verifiedheritageproducers",
        "jobsposted", "jobapplications", "jobsfilled",
        "exportorders", "exportsalesvalue",
        "totalorders", "productssold", "marketplacesalesvalue", "heritageeconomyvalue",
        "tourismbookings", "tourismrevenue", "touristsserved",
        "districtscovered", "villagescovered", "productslisted",
    };

    private readonly INationalDashboardRepository _repository;

    public NationalDashboardService(INationalDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<NationalDashboardOverviewDto> GetOverviewAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        (from, to) = NormaliseWindow(from, to);

        var producers = await _repository.GetProducerMetricsAsync(from, to, cancellationToken);
        var employment = await _repository.GetEmploymentMetricsAsync(from, to, cancellationToken);
        var export = await _repository.GetExportMetricsAsync(from, to, cancellationToken);
        var tourism = await _repository.GetTourismMetricsAsync(from, to, cancellationToken);
        var economy = await _repository.GetHeritageEconomyMetricsAsync(from, to, cancellationToken);
        var coverage = await _repository.GetCoverageMetricsAsync(cancellationToken);

        decimal? previousExport = null;
        double? growthPercent = null;
        if (from.HasValue && to.HasValue)
        {
            var span = to.Value - from.Value;
            var prevFrom = from.Value - span;
            var prev = await _repository.GetExportMetricsAsync(prevFrom, from.Value, cancellationToken);
            previousExport = prev.ExportSalesValue;
            growthPercent = Percentage(prev.ExportSalesValue, export.ExportSalesValue);
        }

        var heritageEconomyValue = economy.MarketplaceSalesValue + tourism.Revenue;

        return new NationalDashboardOverviewDto
        {
            GeneratedAt = DateTime.UtcNow,
            FromDate = from,
            ToDate = to,
            Producers = new ProducerMetricsDto
            {
                Total = producers.Total,
                Active = producers.Active,
                VerifiedHeritage = producers.VerifiedHeritage,
                NewInWindow = producers.NewInWindow,
            },
            Employment = new EmploymentMetricsDto
            {
                JobsPosted = employment.JobsPosted,
                ActiveJobListings = employment.ActiveListings,
                JobApplications = employment.Applications,
                JobsFilled = employment.Filled,
                FillRatePercent = employment.Applications == 0
                    ? 0
                    : Math.Round(employment.Filled * 100.0 / employment.Applications, 2),
            },
            ExportGrowth = new ExportGrowthMetricsDto
            {
                ExporterPartners = export.ExporterPartners,
                ExportOrders = export.ExportOrders,
                ExportSalesValue = export.ExportSalesValue,
                PreviousExportSalesValue = previousExport,
                GrowthPercent = growthPercent,
            },
            Tourism = new TourismMetricsDto
            {
                Bookings = tourism.Bookings,
                CompletedBookings = tourism.CompletedBookings,
                TourismRevenue = tourism.Revenue,
                TouristsServed = tourism.TouristsServed,
                ActiveServices = tourism.ActiveServices,
            },
            HeritageEconomy = new HeritageEconomyMetricsDto
            {
                MarketplaceSalesValue = economy.MarketplaceSalesValue,
                TourismRevenue = tourism.Revenue,
                TotalValue = heritageEconomyValue,
                OrdersPlaced = economy.OrdersPlaced,
                ProductsSold = economy.ProductsSold,
                AverageOrderValue = economy.OrdersPlaced == 0
                    ? 0
                    : Math.Round(economy.MarketplaceSalesValue / economy.OrdersPlaced, 2),
            },
            Coverage = new CoverageMetricsDto
            {
                DistrictsWithProducers = coverage.DistrictsWithProducers,
                TotalDistricts = coverage.TotalDistricts,
                Villages = coverage.Villages,
                ProductsListed = coverage.ProductsListed,
            },
        };
    }

    public async Task<List<DistrictRankingDto>> GetDistrictRankingsAsync(
        string? metric, int top, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var key = (metric ?? "sales").Trim().ToLowerInvariant();
        if (!RankingMetrics.Contains(key))
        {
            throw new ConflictException(
                $"Metric must be one of: {string.Join(", ", RankingMetrics)}.");
        }

        top = Math.Clamp(top, 1, MaxRankingRows);
        (from, to) = NormaliseWindow(from, to);

        var stats = await _repository.GetDistrictStatsAsync(from, to, cancellationToken);

        var ranked = stats
            .Select(s => new
            {
                s.DistrictId,
                s.Name,
                s.Division,
                Value = key switch
                {
                    "producers" => (decimal)s.ProducerCount,
                    "products" => s.ProductCount,
                    "villages" => s.VillageCount,
                    "orders" => s.OrderCount,
                    _ => s.SalesValue,
                },
            })
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.Name)
            .Take(top)
            .ToList();

        return ranked
            .Select((s, i) => new DistrictRankingDto
            {
                Rank = i + 1,
                DistrictId = s.DistrictId,
                Name = s.Name,
                Division = s.Division,
                Metric = key,
                Value = s.Value,
            })
            .ToList();
    }

    public async Task<NationalDashboardSnapshotDto> CaptureSnapshotAsync(
        Guid userId, CreateNationalDashboardSnapshotRequest request, CancellationToken cancellationToken)
    {
        if (request.PeriodEnd <= request.PeriodStart)
        {
            throw new ConflictException("PeriodEnd must be after PeriodStart.");
        }

        var period = Enum.TryParse<DashboardPeriod>(request.Period, true, out var parsed)
            ? parsed
            : throw new ConflictException("Period must be one of: Monthly, Quarterly, Yearly, Custom.");

        var from = DateTime.SpecifyKind(request.PeriodStart, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(request.PeriodEnd, DateTimeKind.Utc);

        var producers = await _repository.GetProducerMetricsAsync(from, to, cancellationToken);
        var employment = await _repository.GetEmploymentMetricsAsync(from, to, cancellationToken);
        var export = await _repository.GetExportMetricsAsync(from, to, cancellationToken);
        var tourism = await _repository.GetTourismMetricsAsync(from, to, cancellationToken);
        var economy = await _repository.GetHeritageEconomyMetricsAsync(from, to, cancellationToken);
        var coverage = await _repository.GetCoverageMetricsAsync(cancellationToken);
        var districtStats = await _repository.GetDistrictStatsAsync(from, to, cancellationToken);

        var now = DateTime.UtcNow;
        var snapshot = new NationalDashboardSnapshot
        {
            Id = Guid.NewGuid(),
            Label = request.Label.Trim(),
            Period = period,
            PeriodStart = from,
            PeriodEnd = to,
            CapturedAt = now,
            TotalProducers = producers.Total,
            ActiveProducers = producers.Active,
            VerifiedHeritageProducers = producers.VerifiedHeritage,
            NewProducers = producers.NewInWindow,
            JobsPosted = employment.JobsPosted,
            JobApplications = employment.Applications,
            JobsFilled = employment.Filled,
            ExporterPartners = export.ExporterPartners,
            ExportOrders = export.ExportOrders,
            ExportSalesValue = export.ExportSalesValue,
            TotalOrders = economy.OrdersPlaced,
            ProductsSold = economy.ProductsSold,
            MarketplaceSalesValue = economy.MarketplaceSalesValue,
            HeritageEconomyValue = economy.MarketplaceSalesValue + tourism.Revenue,
            TourismBookings = tourism.Bookings,
            TourismRevenue = tourism.Revenue,
            TouristsServed = tourism.TouristsServed,
            DistrictsCovered = coverage.DistrictsWithProducers,
            VillagesCovered = coverage.Villages,
            ProductsListed = coverage.ProductsListed,
            Notes = request.Notes?.Trim(),
            GeneratedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var ordered = districtStats
            .OrderByDescending(s => s.SalesValue)
            .ThenBy(s => s.Name)
            .ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            var s = ordered[i];
            snapshot.DistrictStats.Add(new DashboardDistrictStat
            {
                Id = Guid.NewGuid(),
                NationalDashboardSnapshotId = snapshot.Id,
                DistrictId = s.DistrictId,
                DistrictName = s.Name,
                Division = s.Division,
                ProducerCount = s.ProducerCount,
                ProductCount = s.ProductCount,
                VillageCount = s.VillageCount,
                OrderCount = s.OrderCount,
                SalesValue = s.SalesValue,
                Rank = i + 1,
            });
        }

        await _repository.AddSnapshotAsync(snapshot, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetSnapshotByIdAsync(snapshot.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<NationalDashboardSnapshotListItemDto>> GetSnapshotsAsync(
        NationalDashboardSnapshotQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetSnapshotsPagedAsync(query, cancellationToken);

        return new PagedResult<NationalDashboardSnapshotListItemDto>
        {
            Items = items.Select(s => s.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<NationalDashboardSnapshotDto> GetSnapshotByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var snapshot = await _repository.GetSnapshotByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Dashboard snapshot not found.");
        return snapshot.ToDto();
    }

    public async Task DeleteSnapshotAsync(Guid id, CancellationToken cancellationToken)
    {
        var snapshot = await _repository.GetSnapshotByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Dashboard snapshot not found.");

        _repository.RemoveSnapshot(snapshot);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<DashboardTrendDto> GetTrendAsync(
        string metric, string? period, int take, CancellationToken cancellationToken)
    {
        var key = (metric ?? string.Empty).Trim().ToLowerInvariant();
        if (!TrendMetrics.Contains(key))
        {
            throw new ConflictException(
                $"Metric must be one of: {string.Join(", ", TrendMetrics)}.");
        }

        DashboardPeriod? periodFilter = null;
        if (!string.IsNullOrWhiteSpace(period))
        {
            periodFilter = Enum.TryParse<DashboardPeriod>(period, true, out var parsed)
                ? parsed
                : throw new ConflictException("Period must be one of: Monthly, Quarterly, Yearly, Custom.");
        }

        take = Math.Clamp(take, 2, MaxTrendPoints);

        var snapshots = await _repository.GetSnapshotsForTrendAsync(periodFilter, take, cancellationToken);
        // repository returns newest-first; chart oldest-first
        snapshots.Reverse();

        var points = new List<DashboardTrendPointDto>();
        decimal? previous = null;
        foreach (var s in snapshots)
        {
            var value = SelectMetric(s, key);
            points.Add(new DashboardTrendPointDto
            {
                SnapshotId = s.Id,
                Label = s.Label,
                PeriodEnd = s.PeriodEnd,
                Value = value,
                ChangePercent = previous.HasValue ? Percentage(previous.Value, value) : null,
            });
            previous = value;
        }

        return new DashboardTrendDto { Metric = key, Points = points };
    }

    // ---- helpers -------------------------------------------------------

    private static (DateTime? From, DateTime? To) NormaliseWindow(DateTime? from, DateTime? to)
    {
        if (from.HasValue)
        {
            from = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
        }

        if (to.HasValue)
        {
            to = DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
        }

        if (from.HasValue && to.HasValue && to.Value <= from.Value)
        {
            throw new ConflictException("'to' must be after 'from'.");
        }

        return (from, to);
    }

    private static double? Percentage(decimal previous, decimal current)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : null;
        }

        return Math.Round((double)((current - previous) / previous) * 100, 2);
    }

    private static decimal SelectMetric(NationalDashboardSnapshot s, string key) => key switch
    {
        "producers" => s.TotalProducers,
        "activeproducers" => s.ActiveProducers,
        "newproducers" => s.NewProducers,
        "verifiedheritageproducers" => s.VerifiedHeritageProducers,
        "jobsposted" => s.JobsPosted,
        "jobapplications" => s.JobApplications,
        "jobsfilled" => s.JobsFilled,
        "exportorders" => s.ExportOrders,
        "exportsalesvalue" => s.ExportSalesValue,
        "totalorders" => s.TotalOrders,
        "productssold" => s.ProductsSold,
        "marketplacesalesvalue" => s.MarketplaceSalesValue,
        "heritageeconomyvalue" => s.HeritageEconomyValue,
        "tourismbookings" => s.TourismBookings,
        "tourismrevenue" => s.TourismRevenue,
        "touristsserved" => s.TouristsServed,
        "districtscovered" => s.DistrictsCovered,
        "villagescovered" => s.VillagesCovered,
        "productslisted" => s.ProductsListed,
        _ => 0m,
    };
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.BusinessPartner;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Employment;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Repositories;

public class GovAnalyticsRepository : IGovAnalyticsRepository
{
    private static readonly OrderStatus[] CountedOrderStatuses =
    {
        OrderStatus.Pending, OrderStatus.Processing, OrderStatus.Shipped,
        OrderStatus.Delivered, OrderStatus.ReturnRequested,
    };

    private readonly ShilpoHubDbContext _context;

    public GovAnalyticsRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ==== Reports ====================================================

    public async Task AddReportAsync(GovReport report, CancellationToken cancellationToken)
        => await _context.GovReports.AddAsync(report, cancellationToken);

    public void RemoveReport(GovReport report) => _context.GovReports.Remove(report);

    public Task<GovReport?> GetReportByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.GovReports
            .Include(r => r.GeneratedBy)
            .Include(r => r.Sections)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(List<GovReport> Items, int TotalCount)> GetReportsPagedAsync(
        GovReportQueryParameters query, CancellationToken cancellationToken)
    {
        var reports = _context.GovReports.Include(r => r.GeneratedBy).AsQueryable();

        if (TryEnum<GovReportType>(query.ReportType, out var type))
        {
            reports = reports.Where(r => r.ReportType == type);
        }

        if (TryEnum<GovReportStatus>(query.Status, out var status))
        {
            reports = reports.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            reports = reports.Where(r => r.Title.ToLower().Contains(term));
        }

        reports = reports.OrderByDescending(r => r.PeriodEnd).ThenByDescending(r => r.GeneratedAt);

        var totalCount = await reports.CountAsync(cancellationToken);
        var items = await reports
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<GovReportData> GatherReportDataAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var producerQuery = _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer));
        var totalProducers = await producerQuery.CountAsync(cancellationToken);
        var activeProducers = await producerQuery.CountAsync(u => u.IsActive, cancellationToken);
        var newProducers = await producerQuery
            .CountAsync(u => u.CreatedAt >= from && u.CreatedAt < to, cancellationToken);

        var jobsPosted = await _context.JobListings
            .CountAsync(j => j.CreatedAt >= from && j.CreatedAt < to, cancellationToken);
        var jobsFilled = await _context.JobApplications
            .CountAsync(a => a.Status == JobApplicationStatus.Hired
                && a.AppliedAt >= from && a.AppliedAt < to, cancellationToken);

        var items = _context.OrderItems
            .Where(i => CountedOrderStatuses.Contains(i.Order.Status))
            .Where(i => i.Order.CreatedAt >= from && i.Order.CreatedAt < to);
        var orders = await items.Select(i => i.OrderId).Distinct().CountAsync(cancellationToken);
        var marketplaceSales = await items.Select(i => (decimal?)i.LineTotal).SumAsync(cancellationToken) ?? 0m;

        var exporterUserIds = _context.BusinessPartnerProfiles
            .Where(p => p.BusinessType == BusinessType.Exporter)
            .Select(p => p.UserId);
        var exportSales = await _context.Orders
            .Where(o => CountedOrderStatuses.Contains(o.Status))
            .Where(o => o.CreatedAt >= from && o.CreatedAt < to)
            .Where(o => exporterUserIds.Contains(o.UserId))
            .Select(o => (decimal?)o.Total)
            .SumAsync(cancellationToken) ?? 0m;

        var bookings = _context.Bookings.Where(b => b.CreatedAt >= from && b.CreatedAt < to);
        var tourismBookings = await bookings.CountAsync(cancellationToken);
        var tourismRevenue = await bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Select(b => (decimal?)b.TotalPrice)
            .SumAsync(cancellationToken) ?? 0m;

        var districtsCovered = await _context.Products.Select(p => p.DistrictId).Distinct().CountAsync(cancellationToken);
        var villages = await _context.Villages.CountAsync(cancellationToken);

        var flagsRaised = await _context.MonitoringFlags
            .CountAsync(f => f.DetectedAt >= from && f.DetectedAt < to, cancellationToken);
        var flagsOpen = await _context.MonitoringFlags
            .CountAsync(f => f.Status == MonitoringFlagStatus.Open
                || f.Status == MonitoringFlagStatus.UnderReview, cancellationToken);
        var flagsByTypeRaw = await _context.MonitoringFlags
            .Where(f => f.DetectedAt >= from && f.DetectedAt < to)
            .GroupBy(f => f.FlagType)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var flagsByType = flagsByTypeRaw.ToDictionary(x => x.Key.ToString(), x => x.Count);

        var complaintsReceived = await _context.Complaints
            .CountAsync(c => c.CreatedAt >= from && c.CreatedAt < to, cancellationToken);
        var complaintsResolved = await _context.Complaints
            .CountAsync(c => c.ResolvedAt != null && c.ResolvedAt >= from && c.ResolvedAt < to, cancellationToken);

        var programsActive = await _context.FundingPrograms
            .CountAsync(p => p.Status == FundingProgramStatus.Open, cancellationToken);
        var applicationsSubmitted = await _context.FundingApplications
            .CountAsync(a => a.SubmittedAt >= from && a.SubmittedAt < to, cancellationToken);
        var approvedInWindow = _context.FundingApplications
            .Where(a => a.DecisionAt != null && a.DecisionAt >= from && a.DecisionAt < to
                && a.Status == FundingApplicationStatus.Approved);
        var applicationsApproved = await approvedInWindow.CountAsync(cancellationToken);
        var fundingApproved = await approvedInWindow
            .Select(a => (decimal?)(a.ApprovedAmount ?? 0m)).SumAsync(cancellationToken) ?? 0m;
        var fundingDisbursed = await _context.FundingDisbursements
            .Where(d => d.Status == FundingDisbursementStatus.Paid
                && d.PaidAt != null && d.PaidAt >= from && d.PaidAt < to)
            .Select(d => (decimal?)d.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var policyRuns = await _context.PolicySimulations
            .CountAsync(s => s.CreatedAt >= from && s.CreatedAt < to, cancellationToken);
        var indicesComputed = await _context.HeritageIndexRecords
            .CountAsync(r => r.ComputedAt >= from && r.ComputedAt < to, cancellationToken);

        return new GovReportData(
            totalProducers, activeProducers, newProducers,
            jobsPosted, jobsFilled,
            orders, marketplaceSales, exportSales,
            tourismBookings, tourismRevenue,
            districtsCovered, villages,
            flagsRaised, flagsOpen, flagsByType,
            complaintsReceived, complaintsResolved,
            programsActive, applicationsSubmitted, applicationsApproved, fundingApproved, fundingDisbursed,
            policyRuns, indicesComputed);
    }

    // ==== Exports ===================================================

    public async Task AddExportAsync(AnalyticsExport export, CancellationToken cancellationToken)
        => await _context.AnalyticsExports.AddAsync(export, cancellationToken);

    public void RemoveExport(AnalyticsExport export) => _context.AnalyticsExports.Remove(export);

    public Task<AnalyticsExport?> GetExportByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.AnalyticsExports
            .Include(e => e.RequestedBy)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<(List<AnalyticsExport> Items, int TotalCount)> GetExportsPagedAsync(
        AnalyticsExportQueryParameters query, Guid currentUserId, CancellationToken cancellationToken)
    {
        var exports = _context.AnalyticsExports.Include(e => e.RequestedBy).AsQueryable();

        if (query.MineOnly)
        {
            exports = exports.Where(e => e.RequestedByUserId == currentUserId);
        }

        if (TryEnum<AnalyticsExportDataset>(query.Dataset, out var dataset))
        {
            exports = exports.Where(e => e.Dataset == dataset);
        }

        if (TryEnum<AnalyticsExportStatus>(query.Status, out var status))
        {
            exports = exports.Where(e => e.Status == status);
        }

        if (TryEnum<AnalyticsExportFormat>(query.Format, out var format))
        {
            exports = exports.Where(e => e.Format == format);
        }

        exports = exports.OrderByDescending(e => e.RequestedAt);

        var totalCount = await exports.CountAsync(cancellationToken);
        var items = await exports
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> ReportExistsAsync(Guid reportId, CancellationToken cancellationToken)
        => _context.GovReports.AnyAsync(r => r.Id == reportId, cancellationToken);

    // ==== GIS ======================================================

    public async Task<List<GisDistrictAggregate>> GetGisDistrictValuesAsync(
        string metricKey, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var districts = await _context.Districts
            .Select(d => new { d.Id, d.Name, d.Division })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, decimal> values = metricKey switch
        {
            "producers" => (await _context.Products
                    .GroupBy(p => p.DistrictId)
                    .Select(g => new { g.Key, V = g.Select(p => p.ProducerId).Distinct().Count() })
                    .ToListAsync(cancellationToken))
                .ToDictionary(x => x.Key, x => (decimal)x.V),

            "products" => (await _context.Products
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.DistrictId)
                    .Select(g => new { g.Key, V = g.Count() })
                    .ToListAsync(cancellationToken))
                .ToDictionary(x => x.Key, x => (decimal)x.V),

            "villages" => (await _context.Villages
                    .GroupBy(v => v.DistrictId)
                    .Select(g => new { g.Key, V = g.Count() })
                    .ToListAsync(cancellationToken))
                .ToDictionary(x => x.Key, x => (decimal)x.V),

            "risk" => (await _context.HeritageRiskRecords
                    .Where(r => r.DistrictId != null)
                    .GroupBy(r => r.DistrictId!.Value)
                    .Select(g => new
                    {
                        g.Key,
                        V = g.Sum(r =>
                            r.Level == HeritageRiskLevel.Critical ? 7
                            : r.Level == HeritageRiskLevel.High ? 4
                            : r.Level == HeritageRiskLevel.Moderate ? 2
                            : r.Level == HeritageRiskLevel.Low ? 1 : 0),
                    })
                    .ToListAsync(cancellationToken))
                .ToDictionary(x => x.Key, x => (decimal)x.V),

            "orders" => (await OrderItemsInWindow(from, to)
                    .GroupBy(i => i.Product.DistrictId)
                    .Select(g => new { g.Key, V = g.Select(i => i.OrderId).Distinct().Count() })
                    .ToListAsync(cancellationToken))
                .ToDictionary(x => x.Key, x => (decimal)x.V),

            _ => (await OrderItemsInWindow(from, to)
                    .GroupBy(i => i.Product.DistrictId)
                    .Select(g => new { g.Key, V = g.Sum(i => i.LineTotal) })
                    .ToListAsync(cancellationToken))
                .ToDictionary(x => x.Key, x => x.V),
        };

        return districts
            .Select(d => new GisDistrictAggregate(
                d.Id, d.Name, d.Division, values.TryGetValue(d.Id, out var v) ? v : 0m))
            .ToList();
    }

    private IQueryable<OrderItem> OrderItemsInWindow(DateTime? from, DateTime? to)
    {
        var q = _context.OrderItems.Where(i => CountedOrderStatuses.Contains(i.Order.Status));
        if (from.HasValue)
        {
            q = q.Where(i => i.Order.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            q = q.Where(i => i.Order.CreatedAt < to.Value);
        }

        return q;
    }

    // ==== Forecasts ================================================

    public async Task AddForecastAsync(GovForecast forecast, CancellationToken cancellationToken)
        => await _context.GovForecasts.AddAsync(forecast, cancellationToken);

    public void RemoveForecast(GovForecast forecast) => _context.GovForecasts.Remove(forecast);

    public Task<GovForecast?> GetForecastByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.GovForecasts
            .Include(f => f.GeneratedBy)
            .Include(f => f.Points)
            .AsSplitQuery()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<(List<GovForecast> Items, int TotalCount)> GetForecastsPagedAsync(
        GovForecastQueryParameters query, CancellationToken cancellationToken)
    {
        var forecasts = _context.GovForecasts
            .Include(f => f.GeneratedBy)
            .OrderByDescending(f => f.GeneratedAt);

        var totalCount = await forecasts.CountAsync(cancellationToken);
        var items = await forecasts
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<GovForecastGatheredInput> GatherForecastInputAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var from = now.AddMonths(-12);

        var totalProducers = await _context.Users
            .CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer), cancellationToken);
        var jobsFilled = await _context.JobApplications
            .CountAsync(a => a.Status == JobApplicationStatus.Hired, cancellationToken);

        var items = _context.OrderItems
            .Where(i => CountedOrderStatuses.Contains(i.Order.Status))
            .Where(i => i.Order.CreatedAt >= from && i.Order.CreatedAt < now);
        var marketplaceSales = await items.Select(i => (decimal?)i.LineTotal).SumAsync(cancellationToken) ?? 0m;

        var exporterUserIds = _context.BusinessPartnerProfiles
            .Where(p => p.BusinessType == BusinessType.Exporter)
            .Select(p => p.UserId);
        var exportSales = await _context.Orders
            .Where(o => CountedOrderStatuses.Contains(o.Status))
            .Where(o => o.CreatedAt >= from && o.CreatedAt < now)
            .Where(o => exporterUserIds.Contains(o.UserId))
            .Select(o => (decimal?)o.Total)
            .SumAsync(cancellationToken) ?? 0m;

        var tourismRevenue = await _context.Bookings
            .Where(b => b.CreatedAt >= from && b.CreatedAt < now && b.Status == BookingStatus.Completed)
            .Select(b => (decimal?)b.TotalPrice)
            .SumAsync(cancellationToken) ?? 0m;

        var current = new Dictionary<string, decimal>
        {
            ["TotalProducers"] = totalProducers,
            ["JobsFilled"] = jobsFilled,
            ["MarketplaceSalesValue"] = marketplaceSales,
            ["ExportSalesValue"] = exportSales,
            ["TourismRevenue"] = tourismRevenue,
            ["HeritageEconomyValue"] = marketplaceSales + tourismRevenue,
        };

        var snapshots = await _context.NationalDashboardSnapshots
            .OrderBy(s => s.PeriodEnd)
            .Select(s => new
            {
                s.PeriodEnd,
                s.TotalProducers,
                s.JobsFilled,
                s.MarketplaceSalesValue,
                s.ExportSalesValue,
                s.TourismRevenue,
                s.HeritageEconomyValue,
            })
            .ToListAsync(cancellationToken);

        var history = snapshots.Select(s => new GovForecastObservation(
            s.PeriodEnd,
            new Dictionary<string, decimal>
            {
                ["TotalProducers"] = s.TotalProducers,
                ["JobsFilled"] = s.JobsFilled,
                ["MarketplaceSalesValue"] = s.MarketplaceSalesValue,
                ["ExportSalesValue"] = s.ExportSalesValue,
                ["TourismRevenue"] = s.TourismRevenue,
                ["HeritageEconomyValue"] = s.HeritageEconomyValue,
            })).ToList();

        return new GovForecastGatheredInput(now, current, history);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    private static bool TryEnum<T>(string? value, out T result) where T : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out result))
        {
            return true;
        }

        result = default;
        return false;
    }
}

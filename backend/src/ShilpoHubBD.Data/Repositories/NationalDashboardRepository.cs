using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.BusinessPartner;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Employment;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Repositories;

public class NationalDashboardRepository : INationalDashboardRepository
{
    // Order statuses that still represent realised economic value.
    private static readonly OrderStatus[] CountedOrderStatuses =
    {
        OrderStatus.Pending,
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.Delivered,
        OrderStatus.ReturnRequested,
    };

    private readonly ShilpoHubDbContext _context;

    public NationalDashboardRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ---- Producers --------------------------------------------------------

    public async Task<DashboardProducerAggregate> GetProducerMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var producers = _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer));

        var total = await producers.CountAsync(cancellationToken);
        var active = await producers.CountAsync(u => u.IsActive, cancellationToken);

        var verifiedHeritage = await _context.ProducerHeritageIdentities
            .CountAsync(p => p.VerificationStatus == HeritageVerificationStatus.Verified, cancellationToken);

        var newInWindow = await producers
            .Where(u => (from == null || u.CreatedAt >= from) && (to == null || u.CreatedAt < to))
            .CountAsync(cancellationToken);

        return new DashboardProducerAggregate(total, active, verifiedHeritage, newInWindow);
    }

    // ---- Employment ----------------------------------------------------

    public async Task<DashboardEmploymentAggregate> GetEmploymentMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var listings = _context.JobListings
            .Where(j => (from == null || j.CreatedAt >= from) && (to == null || j.CreatedAt < to));

        var jobsPosted = await listings.CountAsync(cancellationToken);
        var activeListings = await _context.JobListings
            .CountAsync(j => j.Status == JobListingStatus.Published, cancellationToken);

        var applications = _context.JobApplications
            .Where(a => (from == null || a.AppliedAt >= from) && (to == null || a.AppliedAt < to));

        var applicationCount = await applications.CountAsync(cancellationToken);
        var filled = await applications.CountAsync(a => a.Status == JobApplicationStatus.Hired, cancellationToken);

        return new DashboardEmploymentAggregate(jobsPosted, activeListings, applicationCount, filled);
    }

    // ---- Export growth ----------------------------------------------

    public async Task<DashboardExportAggregate> GetExportMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var exporterUserIds = _context.BusinessPartnerProfiles
            .Where(p => p.BusinessType == BusinessType.Exporter)
            .Select(p => p.UserId);

        var exporterPartners = await exporterUserIds.CountAsync(cancellationToken);

        var exportOrders = _context.Orders
            .Where(o => CountedOrderStatuses.Contains(o.Status))
            .Where(o => exporterUserIds.Contains(o.UserId))
            .Where(o => (from == null || o.CreatedAt >= from) && (to == null || o.CreatedAt < to));

        var orderCount = await exportOrders.CountAsync(cancellationToken);
        var salesValue = await exportOrders
            .Select(o => (decimal?)o.Total)
            .SumAsync(cancellationToken) ?? 0m;

        return new DashboardExportAggregate(exporterPartners, orderCount, salesValue);
    }

    // ---- Tourism --------------------------------------------------

    public async Task<DashboardTourismAggregate> GetTourismMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var bookings = _context.Bookings
            .Where(b => (from == null || b.CreatedAt >= from) && (to == null || b.CreatedAt < to));

        var bookingCount = await bookings.CountAsync(cancellationToken);
        var completed = bookings.Where(b => b.Status == BookingStatus.Completed);
        var completedCount = await completed.CountAsync(cancellationToken);
        var revenue = await completed.Select(b => (decimal?)b.TotalPrice).SumAsync(cancellationToken) ?? 0m;
        var touristsServed = await completed.Select(b => (int?)b.PartySize).SumAsync(cancellationToken) ?? 0;

        var activeServices = await _context.TouristServices.CountAsync(s => s.IsActive, cancellationToken);

        return new DashboardTourismAggregate(
            bookingCount, completedCount, revenue, touristsServed, activeServices);
    }

    // ---- Heritage economy --------------------------------------

    public async Task<DashboardEconomyAggregate> GetHeritageEconomyMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var orders = _context.Orders
            .Where(o => CountedOrderStatuses.Contains(o.Status))
            .Where(o => (from == null || o.CreatedAt >= from) && (to == null || o.CreatedAt < to));

        var orderCount = await orders.CountAsync(cancellationToken);
        var salesValue = await orders.Select(o => (decimal?)o.Total).SumAsync(cancellationToken) ?? 0m;

        var productsSold = await _context.OrderItems
            .Where(i => CountedOrderStatuses.Contains(i.Order.Status))
            .Where(i => (from == null || i.Order.CreatedAt >= from) && (to == null || i.Order.CreatedAt < to))
            .Select(i => (int?)i.Quantity)
            .SumAsync(cancellationToken) ?? 0;

        return new DashboardEconomyAggregate(salesValue, orderCount, productsSold);
    }

    // ---- Coverage --------------------------------------------

    public async Task<DashboardCoverageAggregate> GetCoverageMetricsAsync(CancellationToken cancellationToken)
    {
        var totalDistricts = await _context.Districts.CountAsync(cancellationToken);
        var districtsWithProducers = await _context.Products
            .Select(p => p.DistrictId)
            .Distinct()
            .CountAsync(cancellationToken);
        var villages = await _context.Villages.CountAsync(cancellationToken);
        var productsListed = await _context.Products.CountAsync(p => p.IsActive, cancellationToken);

        return new DashboardCoverageAggregate(
            districtsWithProducers, totalDistricts, villages, productsListed);
    }

    // ---- District breakdown --------------------------------

    public async Task<List<DashboardDistrictAggregate>> GetDistrictStatsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var districts = await _context.Districts
            .Select(d => new { d.Id, d.Name, d.Division })
            .ToListAsync(cancellationToken);

        var producerCounts = await _context.Products
            .GroupBy(p => p.DistrictId)
            .Select(g => new { DistrictId = g.Key, Producers = g.Select(p => p.ProducerId).Distinct().Count() })
            .ToListAsync(cancellationToken);

        var productCounts = await _context.Products
            .Where(p => p.IsActive)
            .GroupBy(p => p.DistrictId)
            .Select(g => new { DistrictId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var villageCounts = await _context.Villages
            .GroupBy(v => v.DistrictId)
            .Select(g => new { DistrictId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var salesByDistrict = await _context.OrderItems
            .Where(i => CountedOrderStatuses.Contains(i.Order.Status))
            .Where(i => (from == null || i.Order.CreatedAt >= from) && (to == null || i.Order.CreatedAt < to))
            .GroupBy(i => i.Product.DistrictId)
            .Select(g => new
            {
                DistrictId = g.Key,
                Sales = g.Sum(i => i.LineTotal),
                Orders = g.Select(i => i.OrderId).Distinct().Count(),
            })
            .ToListAsync(cancellationToken);

        return districts.Select(d => new DashboardDistrictAggregate(
            d.Id,
            d.Name,
            d.Division,
            producerCounts.FirstOrDefault(x => x.DistrictId == d.Id)?.Producers ?? 0,
            productCounts.FirstOrDefault(x => x.DistrictId == d.Id)?.Count ?? 0,
            villageCounts.FirstOrDefault(x => x.DistrictId == d.Id)?.Count ?? 0,
            salesByDistrict.FirstOrDefault(x => x.DistrictId == d.Id)?.Orders ?? 0,
            salesByDistrict.FirstOrDefault(x => x.DistrictId == d.Id)?.Sales ?? 0m))
            .ToList();
    }

    // ---- Snapshots -----------------------------------------

    public async Task AddSnapshotAsync(NationalDashboardSnapshot snapshot, CancellationToken cancellationToken)
        => await _context.NationalDashboardSnapshots.AddAsync(snapshot, cancellationToken);

    public void RemoveSnapshot(NationalDashboardSnapshot snapshot)
        => _context.NationalDashboardSnapshots.Remove(snapshot);

    public Task<NationalDashboardSnapshot?> GetSnapshotByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.NationalDashboardSnapshots
            .Include(s => s.GeneratedBy)
            .Include(s => s.DistrictStats)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<(List<NationalDashboardSnapshot> Items, int TotalCount)> GetSnapshotsPagedAsync(
        NationalDashboardSnapshotQueryParameters query, CancellationToken cancellationToken)
    {
        var snapshots = _context.NationalDashboardSnapshots
            .Include(s => s.GeneratedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Period)
            && Enum.TryParse<DashboardPeriod>(query.Period, true, out var period))
        {
            snapshots = snapshots.Where(s => s.Period == period);
        }

        snapshots = snapshots.OrderByDescending(s => s.PeriodEnd).ThenByDescending(s => s.CapturedAt);

        var totalCount = await snapshots.CountAsync(cancellationToken);
        var items = await snapshots
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<NationalDashboardSnapshot>> GetSnapshotsForTrendAsync(
        DashboardPeriod? period, int take, CancellationToken cancellationToken)
    {
        var snapshots = _context.NationalDashboardSnapshots.AsQueryable();
        if (period.HasValue)
        {
            snapshots = snapshots.Where(s => s.Period == period.Value);
        }

        return snapshots
            .OrderByDescending(s => s.PeriodEnd)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

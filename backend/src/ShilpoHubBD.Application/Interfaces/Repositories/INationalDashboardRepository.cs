using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

/// <summary>
/// Read-side aggregation over the existing marketplace / employment / tourism / community tables for
/// the Government &amp; NGO National Dashboard, plus persistence for captured snapshots.
/// </summary>
public interface INationalDashboardRepository
{
    Task<DashboardProducerAggregate> GetProducerMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken);

    Task<DashboardEmploymentAggregate> GetEmploymentMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken);

    Task<DashboardExportAggregate> GetExportMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken);

    Task<DashboardTourismAggregate> GetTourismMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken);

    Task<DashboardEconomyAggregate> GetHeritageEconomyMetricsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken);

    Task<DashboardCoverageAggregate> GetCoverageMetricsAsync(CancellationToken cancellationToken);

    Task<List<DashboardDistrictAggregate>> GetDistrictStatsAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken);

    // ---- Snapshots --------------------------------------------------------
    Task AddSnapshotAsync(NationalDashboardSnapshot snapshot, CancellationToken cancellationToken);

    void RemoveSnapshot(NationalDashboardSnapshot snapshot);

    Task<NationalDashboardSnapshot?> GetSnapshotByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<NationalDashboardSnapshot> Items, int TotalCount)> GetSnapshotsPagedAsync(
        NationalDashboardSnapshotQueryParameters query, CancellationToken cancellationToken);

    Task<List<NationalDashboardSnapshot>> GetSnapshotsForTrendAsync(
        DashboardPeriod? period, int take, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public record DashboardProducerAggregate(int Total, int Active, int VerifiedHeritage, int NewInWindow);

public record DashboardEmploymentAggregate(int JobsPosted, int ActiveListings, int Applications, int Filled);

public record DashboardExportAggregate(int ExporterPartners, int ExportOrders, decimal ExportSalesValue);

public record DashboardTourismAggregate(
    int Bookings, int CompletedBookings, decimal Revenue, int TouristsServed, int ActiveServices);

public record DashboardEconomyAggregate(decimal MarketplaceSalesValue, int OrdersPlaced, int ProductsSold);

public record DashboardCoverageAggregate(
    int DistrictsWithProducers, int TotalDistricts, int Villages, int ProductsListed);

public record DashboardDistrictAggregate(
    Guid DistrictId,
    string Name,
    string Division,
    int ProducerCount,
    int ProductCount,
    int VillageCount,
    int OrderCount,
    decimal SalesValue);

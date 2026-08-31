using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

/// <summary>
/// Backs the Government / NGO reporting module: report / export persistence, the cross-module
/// aggregates a report assembles, the district-keyed GIS payload, and forecast persistence + input
/// gathering.
/// </summary>
public interface IGovAnalyticsRepository
{
    // ---- Reports -----------------------------------------------------
    Task AddReportAsync(GovReport report, CancellationToken cancellationToken);

    void RemoveReport(GovReport report);

    Task<GovReport?> GetReportByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<GovReport> Items, int TotalCount)> GetReportsPagedAsync(
        GovReportQueryParameters query, CancellationToken cancellationToken);

    Task<GovReportData> GatherReportDataAsync(DateTime from, DateTime to, CancellationToken cancellationToken);

    // ---- Exports ---------------------------------------------------
    Task AddExportAsync(AnalyticsExport export, CancellationToken cancellationToken);

    void RemoveExport(AnalyticsExport export);

    Task<AnalyticsExport?> GetExportByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<AnalyticsExport> Items, int TotalCount)> GetExportsPagedAsync(
        AnalyticsExportQueryParameters query, Guid currentUserId, CancellationToken cancellationToken);

    Task<bool> ReportExistsAsync(Guid reportId, CancellationToken cancellationToken);

    // ---- GIS -----------------------------------------------------
    Task<List<GisDistrictAggregate>> GetGisDistrictValuesAsync(
        string metricKey, DateTime? from, DateTime? to, CancellationToken cancellationToken);

    // ---- Forecasts --------------------------------------------
    Task AddForecastAsync(GovForecast forecast, CancellationToken cancellationToken);

    void RemoveForecast(GovForecast forecast);

    Task<GovForecast?> GetForecastByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<GovForecast> Items, int TotalCount)> GetForecastsPagedAsync(
        GovForecastQueryParameters query, CancellationToken cancellationToken);

    Task<GovForecastGatheredInput> GatherForecastInputAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public record GovReportData(
    int TotalProducers,
    int ActiveProducers,
    int NewProducers,
    int JobsPosted,
    int JobsFilled,
    int Orders,
    decimal MarketplaceSalesValue,
    decimal ExportSalesValue,
    int TourismBookings,
    decimal TourismRevenue,
    int DistrictsCovered,
    int Villages,
    int FlagsRaised,
    int FlagsOpen,
    Dictionary<string, int> FlagsByType,
    int ComplaintsReceived,
    int ComplaintsResolved,
    int FundingProgramsActive,
    int FundingApplicationsSubmitted,
    int FundingApplicationsApproved,
    decimal FundingApproved,
    decimal FundingDisbursed,
    int PolicySimulationsRun,
    int HeritageIndicesComputed);

public record GisDistrictAggregate(Guid DistrictId, string Name, string Division, decimal Value);

public record GovForecastGatheredInput(
    DateTime AsOf,
    Dictionary<string, decimal> CurrentValues,
    List<GovForecastObservation> History);

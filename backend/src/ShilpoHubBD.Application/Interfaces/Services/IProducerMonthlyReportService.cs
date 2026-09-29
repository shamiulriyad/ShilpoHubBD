using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProducerMonthlyReportService
{
    /// <summary>
    /// Generates the monthly performance snapshot for every producer for the given (year, month).
    /// A producer that already has a report for this month is skipped — reports are never overwritten.
    /// </summary>
    Task<ProducerMonthlyReportGenerationResultDto> GenerateForMonthAsync(int year, int month, CancellationToken cancellationToken);

    Task<PagedResult<ProducerMonthlyReportDto>> GetPagedAsync(ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken);

    Task<ProducerMonthlyReportDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProducerMonthlyReportDto> GetForProducerAsync(Guid producerId, int year, int month, CancellationToken cancellationToken);

    /// <summary>
    /// Compares a producer's month against the one before it. When year/month are omitted, "current"
    /// is the producer's most recently generated report. Throws NotFoundException if the producer has
    /// no report for the requested (or, when omitted, any) month; PreviousMonth is null (not an error)
    /// when there simply isn't an earlier report to compare against.
    /// </summary>
    Task<ProducerMonthlyReportComparisonDto> CompareAsync(Guid producerId, int? year, int? month, CancellationToken cancellationToken);

    // ---- Admin: sharing with Government/NGO --------------------------------

    /// <summary>
    /// Grants the listed Government/NGO users read access to the listed reports. Throws NotFoundException
    /// if any ReportId doesn't exist, ConflictException if any SharedWithUserId isn't a GovernmentNGO
    /// account. A (report, user) pair that's already shared is left as-is, not duplicated.
    /// </summary>
    Task<ProducerMonthlyReportShareResultDto> ShareAsync(
        Guid sharedByUserId, ShareProducerMonthlyReportsRequest request, CancellationToken cancellationToken);

    Task<List<ProducerMonthlyReportShareDto>> GetSharesForReportAsync(Guid reportId, CancellationToken cancellationToken);

    // ---- Government/NGO: restricted read access -----------------------------

    /// <summary>Throws UnauthorizedAccessException if this report has not been shared with governmentUserId.</summary>
    Task<ProducerMonthlyReportDto> GetSharedReportAsync(Guid id, Guid governmentUserId, CancellationToken cancellationToken);

    // ---- Admin: Monthly Producer Intelligence -----------------------------

    Task<PagedResult<ProducerIntelligenceRowDto>> GetIntelligenceListAsync(ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken);

    /// <summary>KPIs over every producer matching the filters (not just one page).</summary>
    Task<ProducerIntelligenceDashboardDto> GetIntelligenceDashboardAsync(ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken);
}

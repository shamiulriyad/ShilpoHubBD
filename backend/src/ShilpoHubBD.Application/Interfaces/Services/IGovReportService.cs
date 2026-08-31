using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IGovReportService
{
    // ---- Reports -----------------------------------------------------
    Task<GovReportDto> GenerateAsync(
        Guid userId, GenerateGovReportRequest request, CancellationToken cancellationToken);

    Task<PagedResult<GovReportListItemDto>> GetReportsAsync(
        GovReportQueryParameters query, CancellationToken cancellationToken);

    Task<GovReportDto> GetReportByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<GovReportDto> UpdateReportAsync(
        Guid userId, Guid id, UpdateGovReportRequest request, CancellationToken cancellationToken);

    Task DeleteReportAsync(Guid id, CancellationToken cancellationToken);

    // ---- GIS -----------------------------------------------------
    Task<GisMapDto> GetGisMapAsync(GisMapQueryParameters query, CancellationToken cancellationToken);

    // ---- Downloadable analytics exports ------------------------
    Task<AnalyticsExportDto> RequestExportAsync(
        Guid userId, CreateAnalyticsExportRequest request, CancellationToken cancellationToken);

    Task<PagedResult<AnalyticsExportDto>> GetExportsAsync(
        Guid userId, AnalyticsExportQueryParameters query, CancellationToken cancellationToken);

    Task<AnalyticsExportDto> GetExportByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AnalyticsExportDto> CompleteExportAsync(
        Guid userId, Guid id, CompleteAnalyticsExportRequest request, CancellationToken cancellationToken);

    Task DeleteExportAsync(Guid id, CancellationToken cancellationToken);
}

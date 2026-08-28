using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IMonitoringService
{
    Task<MonitoringScanResultDto> RunScanAsync(
        Guid userId, RunMonitoringScanRequest request, CancellationToken cancellationToken);

    Task<MonitoringFlagDto> CreateFlagAsync(
        Guid userId, CreateMonitoringFlagRequest request, CancellationToken cancellationToken);

    Task<PagedResult<MonitoringFlagListItemDto>> GetFlagsAsync(
        MonitoringFlagQueryParameters query, CancellationToken cancellationToken);

    Task<MonitoringFlagDto> GetFlagByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<MonitoringFlagDto> UpdateFlagStatusAsync(
        Guid userId, Guid id, UpdateMonitoringFlagStatusRequest request, CancellationToken cancellationToken);

    Task<MonitoringFlagDto> AssignFlagAsync(
        Guid userId, Guid id, AssignMonitoringFlagRequest request, CancellationToken cancellationToken);

    Task<MonitoringFlagDto> AddFlagNoteAsync(
        Guid userId, Guid id, AddMonitoringFlagNoteRequest request, CancellationToken cancellationToken);

    Task DeleteFlagAsync(Guid id, CancellationToken cancellationToken);

    Task<QrMonitoringOverviewDto> GetQrOverviewAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken);
}

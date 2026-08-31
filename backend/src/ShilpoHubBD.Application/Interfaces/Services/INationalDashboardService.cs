using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface INationalDashboardService
{
    Task<NationalDashboardOverviewDto> GetOverviewAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken);

    Task<List<DistrictRankingDto>> GetDistrictRankingsAsync(
        string? metric, int top, DateTime? from, DateTime? to, CancellationToken cancellationToken);

    Task<NationalDashboardSnapshotDto> CaptureSnapshotAsync(
        Guid userId, CreateNationalDashboardSnapshotRequest request, CancellationToken cancellationToken);

    Task<PagedResult<NationalDashboardSnapshotListItemDto>> GetSnapshotsAsync(
        NationalDashboardSnapshotQueryParameters query, CancellationToken cancellationToken);

    Task<NationalDashboardSnapshotDto> GetSnapshotByIdAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteSnapshotAsync(Guid id, CancellationToken cancellationToken);

    Task<DashboardTrendDto> GetTrendAsync(
        string metric, string? period, int take, CancellationToken cancellationToken);
}

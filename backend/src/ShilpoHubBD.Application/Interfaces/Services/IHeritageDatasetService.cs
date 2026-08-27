using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageDatasetService
{
    Task<PagedResult<HeritageDatasetListItemDto>> GetAccessibleAsync(
        HeritageDbAccessContext ctx, HeritageDatasetQueryParameters query, CancellationToken cancellationToken);

    Task<HeritageDatasetDetailDto> GetByIdAsync(HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken);

    Task<HeritageDatasetDetailDto> CreateAsync(
        HeritageDbAccessContext ctx, CreateHeritageDatasetRequest request, CancellationToken cancellationToken);

    Task<HeritageDatasetDetailDto> UpdateAsync(
        HeritageDbAccessContext ctx, Guid datasetId, UpdateHeritageDatasetRequest request, CancellationToken cancellationToken);

    Task<HeritageDatasetDetailDto> RefreshAsync(HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken);

    Task DeleteAsync(HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken);

    Task<List<HeritageDatasetVersionDto>> GetVersionsAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken);

    Task<HeritageDatasetVersionDto> AddVersionAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CreateHeritageDatasetVersionRequest request, CancellationToken cancellationToken);

    Task<List<HeritageDatasetAccessGrantDto>> GetAccessGrantsAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken);

    Task<HeritageDatasetAccessGrantDto> GrantAccessAsync(
        HeritageDbAccessContext ctx, Guid datasetId, GrantHeritageDatasetAccessRequest request, CancellationToken cancellationToken);

    Task RevokeAccessAsync(HeritageDbAccessContext ctx, Guid datasetId, Guid grantId, CancellationToken cancellationToken);

    Task<HeritageDatasetExportDto> CreateExportAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CreateHeritageDatasetExportRequest request, CancellationToken cancellationToken);

    Task<PagedResult<HeritageDatasetExportDto>> GetExportsAsync(
        HeritageDbAccessContext ctx, Guid datasetId, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken);

    Task<HeritageDatasetExportAnalyticsDto> GetExportAnalyticsAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken);

    Task<PagedResult<HeritageDatasetExportDto>> GetMyExportsAsync(
        Guid userId, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken);
}

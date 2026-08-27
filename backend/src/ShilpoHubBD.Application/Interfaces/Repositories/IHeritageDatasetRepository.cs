using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritageDatasetRepository
{
    // Datasets
    Task<HeritageDataset?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HeritageDataset?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
    Task<(List<HeritageDataset> Items, int TotalCount)> GetPagedAccessibleAsync(
        Guid userId, bool canSeeResearcherLevel, HeritageDatasetQueryParameters query, CancellationToken cancellationToken);
    Task AddAsync(HeritageDataset dataset, CancellationToken cancellationToken);
    void Remove(HeritageDataset dataset);

    // Versions
    Task<HeritageDatasetVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken);
    Task<List<HeritageDatasetVersion>> GetVersionsAsync(Guid datasetId, CancellationToken cancellationToken);
    Task<int> GetMaxVersionNumberAsync(Guid datasetId, CancellationToken cancellationToken);
    Task AddVersionAsync(HeritageDatasetVersion version, CancellationToken cancellationToken);

    // Access grants
    Task<HeritageDatasetAccessGrant?> GetGrantAsync(Guid datasetId, Guid userId, CancellationToken cancellationToken);
    Task<HeritageDatasetAccessGrant?> GetGrantByIdAsync(Guid grantId, CancellationToken cancellationToken);
    Task<List<HeritageDatasetAccessGrant>> GetGrantsAsync(Guid datasetId, CancellationToken cancellationToken);
    Task AddGrantAsync(HeritageDatasetAccessGrant grant, CancellationToken cancellationToken);
    void RemoveGrant(HeritageDatasetAccessGrant grant);

    // Exports
    Task<HeritageDatasetExport?> GetExportByIdAsync(Guid exportId, CancellationToken cancellationToken);
    Task<(List<HeritageDatasetExport> Items, int TotalCount)> GetExportsForDatasetAsync(
        Guid datasetId, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken);
    Task<(List<HeritageDatasetExport> Items, int TotalCount)> GetExportsForUserAsync(
        Guid userId, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken);
    Task<HeritageDatasetExportAnalyticsDto> GetExportAnalyticsAsync(Guid datasetId, CancellationToken cancellationToken);
    Task AddExportAsync(HeritageDatasetExport export, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

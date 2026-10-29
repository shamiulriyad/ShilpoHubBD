using System.Linq.Expressions;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.PerformingArts;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPerformingArtRepository
{
    /// <summary>Filtered page ordered featured-first, then DisplayOrder, then English title. Includes District, Media and Sources.</summary>
    Task<(List<PerformingArtRecord> Items, int TotalCount)> SearchAsync(
        Expression<Func<PerformingArtRecord, bool>> filter, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>All matching records (no paging), with District and Sources — for facets and AI retrieval.</summary>
    Task<List<PerformingArtRecord>> ListAsync(Expression<Func<PerformingArtRecord, bool>> filter, CancellationToken cancellationToken);

    Task<PerformingArtRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PerformingArtRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken);
    Task<District?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken);
    Task AddAsync(PerformingArtRecord record, CancellationToken cancellationToken);
    void Remove(PerformingArtRecord record);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

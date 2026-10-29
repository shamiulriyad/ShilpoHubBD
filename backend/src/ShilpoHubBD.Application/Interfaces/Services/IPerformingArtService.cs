using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.PerformingArts;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPerformingArtService
{
    // Public — Published records only.
    Task<PagedResult<PerformingArtListItemDto>> SearchPublishedAsync(PerformingArtQueryParameters query, CancellationToken cancellationToken);
    Task<List<PerformingArtListItemDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken);
    Task<PerformingArtFacetsDto> GetFacetsAsync(CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken);

    // Admin — any status.
    Task<PagedResult<PerformingArtListItemDto>> SearchAdminAsync(PerformingArtQueryParameters query, CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> CreateAsync(SavePerformingArtRequest request, Guid userId, CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> UpdateAsync(Guid id, SavePerformingArtRequest request, CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> SubmitForReviewAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> PublishAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> ReturnToDraftAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> ArchiveAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken);
    Task<PerformingArtDetailDto> RestoreAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

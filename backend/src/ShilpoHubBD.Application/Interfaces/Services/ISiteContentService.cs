using ShilpoHubBD.Application.DTOs.Cms;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISiteContentService
{
    Task<List<SiteContentItemDto>> GetAllAsync(string? group, bool includeInactive, CancellationToken cancellationToken);
    Task<SiteContentItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SiteContentItemDto> CreateAsync(SaveSiteContentItemRequest request, CancellationToken cancellationToken);
    Task<SiteContentItemDto> UpdateAsync(Guid id, SaveSiteContentItemRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

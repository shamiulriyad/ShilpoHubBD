using ShilpoHubBD.Application.DTOs.Cms;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHomepageSectionService
{
    Task<List<HomepageSectionDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<HomepageSectionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HomepageSectionDto> CreateAsync(CreateHomepageSectionRequest request, CancellationToken cancellationToken);
    Task<HomepageSectionDto> UpdateAsync(Guid id, UpdateHomepageSectionRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICraftStoryService
{
    Task<CraftStoryDto> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<CraftStoryDto> CreateAsync(CreateCraftStoryRequest request, CancellationToken cancellationToken);
    Task<CraftStoryDto> UpdateAsync(Guid id, UpdateCraftStoryRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

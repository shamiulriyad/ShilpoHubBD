using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IVillageService
{
    Task<List<VillageDto>> GetAllAsync(Guid? currentUserId, CancellationToken cancellationToken);
    Task<VillageDto> GetByIdAsync(Guid id, Guid? currentUserId, CancellationToken cancellationToken);
    Task<VillageDto> CreateAsync(CreateVillageRequest request, CancellationToken cancellationToken);
    Task<VillageDto> UpdateAsync(Guid id, UpdateVillageRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<List<VillageDto>> GetMyFavoritesAsync(Guid userId, CancellationToken cancellationToken);
    Task FavoriteAsync(Guid userId, Guid villageId, CancellationToken cancellationToken);
    Task UnfavoriteAsync(Guid userId, Guid villageId, CancellationToken cancellationToken);
}

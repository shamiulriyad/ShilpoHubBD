using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IVillageRepository
{
    Task<List<Village>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<Village?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Village village, CancellationToken cancellationToken);
    void Remove(Village village);
    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<List<VillageFavorite>> GetFavoritesByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<HashSet<Guid>> GetFavoritedVillageIdsAsync(Guid userId, CancellationToken cancellationToken);
    Task<VillageFavorite?> GetFavoriteAsync(Guid userId, Guid villageId, CancellationToken cancellationToken);
    Task AddFavoriteAsync(VillageFavorite favorite, CancellationToken cancellationToken);
    void RemoveFavorite(VillageFavorite favorite);
}

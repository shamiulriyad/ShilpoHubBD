using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Repositories;

public class VillageRepository : IVillageRepository
{
    private readonly ShilpoHubDbContext _context;

    public VillageRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Village> WithDetails()
        => _context.Villages.Include(v => v.District);

    public Task<List<Village>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
        => WithDetails()
            .Where(v => includeInactive || v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);

    public Task<Village?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task AddAsync(Village village, CancellationToken cancellationToken)
        => await _context.Villages.AddAsync(village, cancellationToken);

    public void Remove(Village village)
        => _context.Villages.Remove(village);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    public Task<List<VillageFavorite>> GetFavoritesByUserAsync(Guid userId, CancellationToken cancellationToken)
        => _context.VillageFavorites
            .Include(f => f.Village).ThenInclude(v => v.District)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<HashSet<Guid>> GetFavoritedVillageIdsAsync(Guid userId, CancellationToken cancellationToken)
        => (await _context.VillageFavorites
            .Where(f => f.UserId == userId)
            .Select(f => f.VillageId)
            .ToListAsync(cancellationToken)).ToHashSet();

    public Task<VillageFavorite?> GetFavoriteAsync(Guid userId, Guid villageId, CancellationToken cancellationToken)
        => _context.VillageFavorites.FirstOrDefaultAsync(f => f.UserId == userId && f.VillageId == villageId, cancellationToken);

    public async Task AddFavoriteAsync(VillageFavorite favorite, CancellationToken cancellationToken)
        => await _context.VillageFavorites.AddAsync(favorite, cancellationToken);

    public void RemoveFavorite(VillageFavorite favorite)
        => _context.VillageFavorites.Remove(favorite);
}

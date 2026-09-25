using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Repositories;

public class CraftHeritageRepository : ICraftHeritageRepository
{
    private readonly ShilpoHubDbContext _context;

    public CraftHeritageRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<CraftHeritageEntry>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = _context.CraftHeritageEntries.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        return query.OrderBy(e => e.DisplayOrder).ThenBy(e => e.Name).ToListAsync(cancellationToken);
    }

    public Task<CraftHeritageEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.CraftHeritageEntries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken)
        => _context.CraftHeritageEntries.AnyAsync(e => e.Slug == slug && (exceptId == null || e.Id != exceptId), cancellationToken);

    public async Task AddAsync(CraftHeritageEntry entry, CancellationToken cancellationToken)
        => await _context.CraftHeritageEntries.AddAsync(entry, cancellationToken);

    public void Remove(CraftHeritageEntry entry) => _context.CraftHeritageEntries.Remove(entry);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}

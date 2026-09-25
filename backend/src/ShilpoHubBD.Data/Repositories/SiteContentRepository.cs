using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Repositories;

public class SiteContentRepository : ISiteContentRepository
{
    private readonly ShilpoHubDbContext _context;

    public SiteContentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<SiteContentItem>> GetAllAsync(string? group, bool includeInactive, CancellationToken cancellationToken)
    {
        var query = _context.SiteContentItems.AsQueryable();
        if (group is not null)
        {
            query = query.Where(i => i.Group == group);
        }

        if (!includeInactive)
        {
            query = query.Where(i => i.IsActive);
        }

        return query.OrderBy(i => i.Group).ThenBy(i => i.DisplayOrder).ThenBy(i => i.Title).ToListAsync(cancellationToken);
    }

    public Task<SiteContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.SiteContentItems.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task AddAsync(SiteContentItem item, CancellationToken cancellationToken)
        => await _context.SiteContentItems.AddAsync(item, cancellationToken);

    public void Remove(SiteContentItem item) => _context.SiteContentItems.Remove(item);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}

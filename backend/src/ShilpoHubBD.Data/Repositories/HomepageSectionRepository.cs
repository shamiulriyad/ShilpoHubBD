using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Repositories;

public class HomepageSectionRepository : IHomepageSectionRepository
{
    private readonly ShilpoHubDbContext _context;

    public HomepageSectionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<HomepageSection>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var sections = _context.HomepageSections.AsQueryable();

        if (!includeInactive)
        {
            sections = sections.Where(s => s.IsActive);
        }

        return sections.OrderBy(s => s.DisplayOrder).ToListAsync(cancellationToken);
    }

    public Task<HomepageSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.HomepageSections.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<bool> ExistsBySectionKeyAsync(string sectionKey, CancellationToken cancellationToken)
        => _context.HomepageSections.AnyAsync(s => s.SectionKey == sectionKey, cancellationToken);

    public async Task AddAsync(HomepageSection section, CancellationToken cancellationToken)
        => await _context.HomepageSections.AddAsync(section, cancellationToken);

    public void Remove(HomepageSection section)
        => _context.HomepageSections.Remove(section);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

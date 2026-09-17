using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Repositories;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly ShilpoHubDbContext _context;

    public AnnouncementRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<Announcement>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var announcements = _context.Announcements.AsQueryable();

        if (activeOnly)
        {
            var now = DateTime.UtcNow;
            announcements = announcements.Where(a => a.IsActive
                && (a.StartsAt == null || a.StartsAt <= now)
                && (a.EndsAt == null || a.EndsAt >= now));
        }

        return announcements.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task<Announcement?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Announcements.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Announcement announcement, CancellationToken cancellationToken)
        => await _context.Announcements.AddAsync(announcement, cancellationToken);

    public void Remove(Announcement announcement)
        => _context.Announcements.Remove(announcement);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

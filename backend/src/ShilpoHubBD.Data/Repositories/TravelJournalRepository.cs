using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Data.Repositories;

public class TravelJournalRepository : ITravelJournalRepository
{
    private readonly ShilpoHubDbContext _context;

    public TravelJournalRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<TravelJournalEntry> WithDetails()
        => _context.TravelJournalEntries.Include(j => j.HeritagePlace);

    public Task<List<TravelJournalEntry>> GetMyEntriesAsync(Guid userId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<TravelJournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task AddAsync(TravelJournalEntry entry, CancellationToken cancellationToken)
        => await _context.TravelJournalEntries.AddAsync(entry, cancellationToken);

    public void Remove(TravelJournalEntry entry)
        => _context.TravelJournalEntries.Remove(entry);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

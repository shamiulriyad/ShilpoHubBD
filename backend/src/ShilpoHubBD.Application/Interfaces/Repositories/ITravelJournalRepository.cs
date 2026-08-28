using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ITravelJournalRepository
{
    Task<List<TravelJournalEntry>> GetMyEntriesAsync(Guid userId, CancellationToken cancellationToken);
    Task<TravelJournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(TravelJournalEntry entry, CancellationToken cancellationToken);
    void Remove(TravelJournalEntry entry);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

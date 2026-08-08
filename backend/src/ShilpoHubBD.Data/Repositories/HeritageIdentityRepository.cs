using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageIdentityRepository : IHeritageIdentityRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageIdentityRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProducerHeritageIdentity> WithDetails()
        => _context.ProducerHeritageIdentities
            .Include(h => h.Producer)
            .Include(h => h.VerifiedBy)
            .Include(h => h.District)
            .Include(h => h.FamilyMembers)
            .Include(h => h.SkillTimeline)
            .Include(h => h.Awards)
            .Include(h => h.Certifications)
            .Include(h => h.StoryArchive)
            .AsSplitQuery();

    public Task<ProducerHeritageIdentity?> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(h => h.ProducerId == producerId, cancellationToken);

    public Task<bool> ExistsByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
        => _context.ProducerHeritageIdentities.AnyAsync(h => h.ProducerId == producerId, cancellationToken);

    public Task<bool> ExistsByHeritageIdNumberAsync(string heritageIdNumber, CancellationToken cancellationToken)
        => _context.ProducerHeritageIdentities.AnyAsync(h => h.HeritageIdNumber == heritageIdNumber, cancellationToken);

    public async Task<(List<ProducerHeritageIdentity> Items, int TotalCount)> GetPagedVerifiedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = WithDetails()
            .Where(h => h.VerificationStatus == HeritageVerificationStatus.Verified)
            .OrderByDescending(h => h.LegacyScore);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<ScoreHistoryEntry> Items, int TotalCount)> GetScoreHistoryAsync(
        Guid producerHeritageIdentityId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.HeritageScoreHistory
            .Where(e => e.ProducerHeritageIdentityId == producerHeritageIdentityId)
            .OrderByDescending(e => e.CalculatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ProducerHeritageIdentity identity, CancellationToken cancellationToken)
        => await _context.ProducerHeritageIdentities.AddAsync(identity, cancellationToken);

    public async Task AddScoreHistoryEntryAsync(ScoreHistoryEntry entry, CancellationToken cancellationToken)
        => await _context.HeritageScoreHistory.AddAsync(entry, cancellationToken);

    public void Remove(ProducerHeritageIdentity identity)
        => _context.ProducerHeritageIdentities.Remove(identity);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

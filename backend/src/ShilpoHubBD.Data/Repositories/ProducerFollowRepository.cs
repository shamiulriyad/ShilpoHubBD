using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerFollowRepository : IProducerFollowRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerFollowRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<ProducerFollow>> GetByFollowerAsync(Guid followerId, CancellationToken cancellationToken)
        => _context.ProducerFollows
            .Include(f => f.Producer)
            .Where(f => f.FollowerId == followerId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<ProducerFollow?> GetAsync(Guid followerId, Guid producerId, CancellationToken cancellationToken)
        => _context.ProducerFollows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.ProducerId == producerId, cancellationToken);

    public async Task AddAsync(ProducerFollow follow, CancellationToken cancellationToken)
        => await _context.ProducerFollows.AddAsync(follow, cancellationToken);

    public void Remove(ProducerFollow follow)
        => _context.ProducerFollows.Remove(follow);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

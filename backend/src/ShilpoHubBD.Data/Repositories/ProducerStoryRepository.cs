using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerStoryRepository : IProducerStoryRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerStoryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<ProducerStory?> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
        => _context.ProducerStories
            .Include(s => s.Chapters)
            .FirstOrDefaultAsync(s => s.ProducerId == producerId, cancellationToken);

    public Task<bool> ExistsByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
        => _context.ProducerStories.AnyAsync(s => s.ProducerId == producerId, cancellationToken);

    public Task<bool> ExistsByHeritageIdAsync(string heritageId, CancellationToken cancellationToken)
        => _context.ProducerStories.AnyAsync(s => s.HeritageId == heritageId, cancellationToken);

    public async Task AddAsync(ProducerStory story, CancellationToken cancellationToken)
        => await _context.ProducerStories.AddAsync(story, cancellationToken);

    public void Remove(ProducerStory story)
        => _context.ProducerStories.Remove(story);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class CraftStoryRepository : ICraftStoryRepository
{
    private readonly ShilpoHubDbContext _context;

    public CraftStoryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<CraftStory> WithDetails()
        => _context.CraftStories
            .Include(s => s.Category)
            .Include(s => s.Chapters);

    public Task<CraftStory?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<CraftStory?> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(s => s.CategoryId == categoryId, cancellationToken);

    public Task<bool> ExistsByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken)
        => _context.CraftStories.AnyAsync(s => s.CategoryId == categoryId, cancellationToken);

    public async Task AddAsync(CraftStory story, CancellationToken cancellationToken)
        => await _context.CraftStories.AddAsync(story, cancellationToken);

    public void Remove(CraftStory story)
        => _context.CraftStories.Remove(story);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Data.Repositories;

public class CulturalStoryRepository : ICulturalStoryRepository
{
    private readonly ShilpoHubDbContext _context;

    public CulturalStoryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<CulturalStory> WithDetails()
        => _context.CulturalStories
            .Include(s => s.HeritagePlace)
            .Include(s => s.Chapters)
            .AsSplitQuery();

    public async Task<(List<CulturalStory> Items, int TotalCount)> GetPagedAsync(
        CulturalStoryQueryParameters query, CancellationToken cancellationToken)
    {
        var stories = WithDetails().Where(s => s.IsActive);

        if (query.HeritagePlaceId.HasValue)
        {
            stories = stories.Where(s => s.HeritagePlaceId == query.HeritagePlaceId.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            stories = stories.Where(s => s.IsFeatured == query.IsFeatured.Value);
        }

        stories = stories.OrderByDescending(s => s.IsFeatured).ThenBy(s => s.Title);

        var totalCount = await stories.CountAsync(cancellationToken);
        var items = await stories
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<CulturalStory?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(CulturalStory story, CancellationToken cancellationToken)
        => await _context.CulturalStories.AddAsync(story, cancellationToken);

    public void Remove(CulturalStory story)
        => _context.CulturalStories.Remove(story);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

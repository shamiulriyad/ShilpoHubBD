using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Marketplace;

public class CraftStoryService : ICraftStoryService
{
    private readonly ICraftStoryRepository _craftStoryRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CraftStoryService(ICraftStoryRepository craftStoryRepository, ICategoryRepository categoryRepository)
    {
        _craftStoryRepository = craftStoryRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CraftStoryDto> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var story = await _craftStoryRepository.GetByCategoryIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("Craft story not found for this category.");

        return ToDto(story);
    }

    public async Task<CraftStoryDto> CreateAsync(CreateCraftStoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (await _craftStoryRepository.ExistsByCategoryIdAsync(request.CategoryId, cancellationToken))
        {
            throw new ConflictException("This category already has a craft story.");
        }

        var now = DateTime.UtcNow;
        var story = new CraftStory
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Origin = request.Origin.Trim(),
            Since = request.Since,
            Summary = request.Summary.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        AttachChapters(story, request.Chapters);

        await _craftStoryRepository.AddAsync(story, cancellationToken);
        await _craftStoryRepository.SaveChangesAsync(cancellationToken);

        story.Category = category;
        return ToDto(story);
    }

    public async Task<CraftStoryDto> UpdateAsync(Guid id, UpdateCraftStoryRequest request, CancellationToken cancellationToken)
    {
        var story = await _craftStoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Craft story not found.");

        story.Origin = request.Origin.Trim();
        story.Since = request.Since;
        story.Summary = request.Summary.Trim();
        story.UpdatedAt = DateTime.UtcNow;

        story.Chapters.Clear();
        AttachChapters(story, request.Chapters);

        await _craftStoryRepository.SaveChangesAsync(cancellationToken);

        return ToDto(story);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var story = await _craftStoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Craft story not found.");

        _craftStoryRepository.Remove(story);
        await _craftStoryRepository.SaveChangesAsync(cancellationToken);
    }

    private static void AttachChapters(CraftStory story, List<StoryChapterInput> chapters)
    {
        for (var i = 0; i < chapters.Count; i++)
        {
            story.Chapters.Add(new CraftStoryChapter
            {
                Id = Guid.NewGuid(),
                Heading = chapters[i].Heading.Trim(),
                Body = chapters[i].Body.Trim(),
                DisplayOrder = i,
            });
        }
    }

    private static CraftStoryDto ToDto(CraftStory story) => new()
    {
        Id = story.Id,
        CategoryId = story.CategoryId,
        CategoryName = story.Category.Name,
        Origin = story.Origin,
        Since = story.Since,
        Summary = story.Summary,
        Chapters = story.Chapters
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new StoryChapterDto { Heading = c.Heading, Body = c.Body, DisplayOrder = c.DisplayOrder })
            .ToList(),
    };
}

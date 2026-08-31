using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.Services.ArVr;

public class CulturalStoryService : ICulturalStoryService
{
    private readonly ICulturalStoryRepository _storyRepository;
    private readonly IHeritagePlaceRepository _heritagePlaceRepository;

    public CulturalStoryService(ICulturalStoryRepository storyRepository, IHeritagePlaceRepository heritagePlaceRepository)
    {
        _storyRepository = storyRepository;
        _heritagePlaceRepository = heritagePlaceRepository;
    }

    public async Task<PagedResult<CulturalStoryDto>> GetPagedAsync(CulturalStoryQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _storyRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<CulturalStoryDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<CulturalStoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Cultural story not found.");
        return ToDto(story);
    }

    public async Task<CulturalStoryDto> CreateAsync(CreateCulturalStoryRequest request, CancellationToken cancellationToken)
    {
        if (request.HeritagePlaceId.HasValue
            && await _heritagePlaceRepository.GetByIdAsync(request.HeritagePlaceId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage place not found.");
        }

        var now = DateTime.UtcNow;
        var story = new CulturalStory
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            CoverImageUrl = request.CoverImageUrl?.Trim(),
            IsFeatured = request.IsFeatured,
            IsActive = true,
            HeritagePlaceId = request.HeritagePlaceId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        AttachChapters(story, request.Chapters);

        await _storyRepository.AddAsync(story, cancellationToken);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        var created = await _storyRepository.GetByIdAsync(story.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<CulturalStoryDto> UpdateAsync(Guid id, UpdateCulturalStoryRequest request, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Cultural story not found.");

        if (request.HeritagePlaceId.HasValue
            && await _heritagePlaceRepository.GetByIdAsync(request.HeritagePlaceId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage place not found.");
        }

        story.Title = request.Title.Trim();
        story.Summary = request.Summary.Trim();
        story.CoverImageUrl = request.CoverImageUrl?.Trim();
        story.IsFeatured = request.IsFeatured;
        story.IsActive = request.IsActive;
        story.HeritagePlaceId = request.HeritagePlaceId;
        story.UpdatedAt = DateTime.UtcNow;

        story.Chapters.Clear();
        AttachChapters(story, request.Chapters);

        await _storyRepository.SaveChangesAsync(cancellationToken);

        var updated = await _storyRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Cultural story not found.");

        _storyRepository.Remove(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);
    }

    private static void AttachChapters(CulturalStory story, List<CulturalStoryChapterInput> chapters)
    {
        for (var i = 0; i < chapters.Count; i++)
        {
            story.Chapters.Add(new CulturalStoryChapter
            {
                Id = Guid.NewGuid(),
                Heading = chapters[i].Heading.Trim(),
                Body = chapters[i].Body.Trim(),
                MediaUrl = chapters[i].MediaUrl?.Trim(),
                MediaType = chapters[i].MediaType,
                DisplayOrder = i,
            });
        }
    }

    private static CulturalStoryDto ToDto(CulturalStory story) => new()
    {
        Id = story.Id,
        Title = story.Title,
        Summary = story.Summary,
        CoverImageUrl = story.CoverImageUrl,
        IsFeatured = story.IsFeatured,
        IsActive = story.IsActive,
        HeritagePlaceId = story.HeritagePlaceId,
        HeritagePlaceName = story.HeritagePlace?.Name,
        Chapters = story.Chapters
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CulturalStoryChapterDto
            {
                Heading = c.Heading,
                Body = c.Body,
                MediaUrl = c.MediaUrl,
                MediaType = c.MediaType?.ToString(),
                DisplayOrder = c.DisplayOrder,
            })
            .ToList(),
        CreatedAt = story.CreatedAt,
        UpdatedAt = story.UpdatedAt,
    };
}

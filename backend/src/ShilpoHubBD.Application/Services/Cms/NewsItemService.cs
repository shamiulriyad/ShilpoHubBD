using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Services.Cms;

public class NewsItemService : INewsItemService
{
    private readonly INewsItemRepository _repository;

    public NewsItemService(INewsItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<NewsItemListItemDto>> GetPagedAsync(NewsItemQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, publishedOnly: true, cancellationToken);
        return new PagedResult<NewsItemListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PagedResult<NewsItemListItemDto>> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 12 : pageSize;

        var (items, totalCount) = await _repository.GetDraftsAsync(page, pageSize, cancellationToken);
        return new PagedResult<NewsItemListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<NewsItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("News item not found.");
        return ToDto(item);
    }

    public async Task<NewsItemDto> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var item = await _repository.GetBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException("News item not found.");
        return ToDto(item);
    }

    public async Task<NewsItemDto> CreateAsync(CreateNewsItemRequest request, CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        var now = DateTime.UtcNow;

        var item = new NewsItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = slug,
            Summary = request.Summary.Trim(),
            Content = request.Content.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            Source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim(),
            IsPublished = request.Publish,
            PublishedAt = request.Publish ? now : null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task<NewsItemDto> UpdateAsync(Guid id, UpdateNewsItemRequest request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("News item not found.");

        if (!item.Title.Equals(request.Title.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            item.Slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        }

        item.Title = request.Title.Trim();
        item.Summary = request.Summary.Trim();
        item.Content = request.Content.Trim();
        item.ImageUrl = request.ImageUrl?.Trim();
        item.Source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim();

        if (request.IsPublished && !item.IsPublished)
        {
            item.PublishedAt = DateTime.UtcNow;
        }

        item.IsPublished = request.IsPublished;
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("News item not found.");

        _repository.Remove(item);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(title);
        var slug = baseSlug;
        var suffix = 2;

        while (await _repository.ExistsBySlugAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static NewsItemListItemDto ToListItemDto(NewsItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Slug = item.Slug,
        Summary = item.Summary,
        ImageUrl = item.ImageUrl,
        Source = item.Source,
        IsPublished = item.IsPublished,
        PublishedAt = item.PublishedAt,
    };

    private static NewsItemDto ToDto(NewsItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Slug = item.Slug,
        Summary = item.Summary,
        Content = item.Content,
        ImageUrl = item.ImageUrl,
        Source = item.Source,
        IsPublished = item.IsPublished,
        PublishedAt = item.PublishedAt,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt,
    };
}

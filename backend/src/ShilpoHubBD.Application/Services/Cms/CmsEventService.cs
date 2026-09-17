using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Services.Cms;

public class CmsEventService : ICmsEventService
{
    private readonly ICmsEventRepository _repository;

    public CmsEventService(ICmsEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<CmsEventListItemDto>> GetPagedAsync(CmsEventQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, publishedOnly: true, cancellationToken);
        return new PagedResult<CmsEventListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PagedResult<CmsEventListItemDto>> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 12 : pageSize;

        var (items, totalCount) = await _repository.GetDraftsAsync(page, pageSize, cancellationToken);
        return new PagedResult<CmsEventListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<CmsEventDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var cmsEvent = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Event not found.");
        return ToDto(cmsEvent);
    }

    public async Task<CmsEventDto> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var cmsEvent = await _repository.GetBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException("Event not found.");
        return ToDto(cmsEvent);
    }

    public async Task<CmsEventDto> CreateAsync(CreateCmsEventRequest request, CancellationToken cancellationToken)
    {
        if (request.EndDate < request.StartDate)
        {
            throw new ConflictException("End date cannot be earlier than start date.");
        }

        var slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        var now = DateTime.UtcNow;

        var cmsEvent = new CmsEvent
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
            IsPublished = request.Publish,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(cmsEvent, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(cmsEvent);
    }

    public async Task<CmsEventDto> UpdateAsync(Guid id, UpdateCmsEventRequest request, CancellationToken cancellationToken)
    {
        var cmsEvent = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Event not found.");

        if (request.EndDate < request.StartDate)
        {
            throw new ConflictException("End date cannot be earlier than start date.");
        }

        if (!cmsEvent.Title.Equals(request.Title.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            cmsEvent.Slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        }

        cmsEvent.Title = request.Title.Trim();
        cmsEvent.Description = request.Description.Trim();
        cmsEvent.ImageUrl = request.ImageUrl?.Trim();
        cmsEvent.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        cmsEvent.StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        cmsEvent.EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);
        cmsEvent.IsPublished = request.IsPublished;
        cmsEvent.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(cmsEvent);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var cmsEvent = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Event not found.");

        _repository.Remove(cmsEvent);
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

    private static CmsEventListItemDto ToListItemDto(CmsEvent cmsEvent) => new()
    {
        Id = cmsEvent.Id,
        Title = cmsEvent.Title,
        Slug = cmsEvent.Slug,
        ImageUrl = cmsEvent.ImageUrl,
        Location = cmsEvent.Location,
        StartDate = cmsEvent.StartDate,
        EndDate = cmsEvent.EndDate,
        IsPublished = cmsEvent.IsPublished,
    };

    private static CmsEventDto ToDto(CmsEvent cmsEvent) => new()
    {
        Id = cmsEvent.Id,
        Title = cmsEvent.Title,
        Slug = cmsEvent.Slug,
        Description = cmsEvent.Description,
        ImageUrl = cmsEvent.ImageUrl,
        Location = cmsEvent.Location,
        StartDate = cmsEvent.StartDate,
        EndDate = cmsEvent.EndDate,
        IsPublished = cmsEvent.IsPublished,
        CreatedAt = cmsEvent.CreatedAt,
        UpdatedAt = cmsEvent.UpdatedAt,
    };
}

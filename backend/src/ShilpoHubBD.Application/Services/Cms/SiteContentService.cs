using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Services.Cms;

/// <summary>Admin-managed public-site copy: About page, footer links and travel resources.</summary>
public class SiteContentService : ISiteContentService
{
    private readonly ISiteContentRepository _repository;

    public SiteContentService(ISiteContentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SiteContentItemDto>> GetAllAsync(string? group, bool includeInactive, CancellationToken cancellationToken)
        => (await _repository.GetAllAsync(string.IsNullOrWhiteSpace(group) ? null : group.Trim(), includeInactive, cancellationToken)).Select(ToDto).ToList();

    public async Task<SiteContentItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => ToDto(await LoadAsync(id, cancellationToken));

    public async Task<SiteContentItemDto> CreateAsync(SaveSiteContentItemRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var item = new SiteContentItem { Id = Guid.NewGuid(), CreatedAt = now };
        Apply(item, request, now);
        item.IsActive = true;

        await _repository.AddAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(item);
    }

    public async Task<SiteContentItemDto> UpdateAsync(Guid id, SaveSiteContentItemRequest request, CancellationToken cancellationToken)
    {
        var item = await LoadAsync(id, cancellationToken);
        Apply(item, request, DateTime.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _repository.Remove(await LoadAsync(id, cancellationToken));
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<SiteContentItem> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Site content item not found.");

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Apply(SiteContentItem item, SaveSiteContentItemRequest request, DateTime now)
    {
        item.Group = request.Group.Trim();
        item.Title = request.Title.Trim();
        item.Subtitle = Clean(request.Subtitle);
        item.Body = Clean(request.Body);
        item.LinkUrl = Clean(request.LinkUrl);
        item.Extra = Clean(request.Extra);
        item.DisplayOrder = request.DisplayOrder;
        item.IsActive = request.IsActive;
        item.UpdatedAt = now;
    }

    private static SiteContentItemDto ToDto(SiteContentItem i) => new()
    {
        Id = i.Id, Group = i.Group, Title = i.Title, Subtitle = i.Subtitle, Body = i.Body, LinkUrl = i.LinkUrl,
        Extra = i.Extra, DisplayOrder = i.DisplayOrder, IsActive = i.IsActive, CreatedAt = i.CreatedAt, UpdatedAt = i.UpdatedAt,
    };
}

using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Services.HeritageDiscovery;

/// <summary>Admin-curated craft heritage reference articles shown on the Explore &gt; Crafts pages.</summary>
public class CraftHeritageService : ICraftHeritageService
{
    private readonly ICraftHeritageRepository _repository;

    public CraftHeritageService(ICraftHeritageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CraftHeritageEntryDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
        => (await _repository.GetAllAsync(includeInactive, cancellationToken)).Select(ToDto).ToList();

    public async Task<CraftHeritageEntryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => ToDto(await LoadAsync(id, cancellationToken));

    public async Task<CraftHeritageEntryDto> CreateAsync(SaveCraftHeritageEntryRequest request, CancellationToken cancellationToken)
    {
        var slug = NormaliseSlug(request.Slug);
        if (await _repository.SlugExistsAsync(slug, null, cancellationToken))
        {
            throw new ConflictException("A craft heritage entry with this slug already exists.");
        }

        var now = DateTime.UtcNow;
        var entry = new CraftHeritageEntry { Id = Guid.NewGuid(), CreatedAt = now };
        Apply(entry, request, slug, now);
        entry.IsActive = true;

        await _repository.AddAsync(entry, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(entry);
    }

    public async Task<CraftHeritageEntryDto> UpdateAsync(Guid id, SaveCraftHeritageEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await LoadAsync(id, cancellationToken);
        var slug = NormaliseSlug(request.Slug);
        if (await _repository.SlugExistsAsync(slug, id, cancellationToken))
        {
            throw new ConflictException("A craft heritage entry with this slug already exists.");
        }

        Apply(entry, request, slug, DateTime.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(entry);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _repository.Remove(await LoadAsync(id, cancellationToken));
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<CraftHeritageEntry> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Craft heritage entry not found.");

    private static string NormaliseSlug(string slug) => slug.Trim().ToLowerInvariant();

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Apply(CraftHeritageEntry e, SaveCraftHeritageEntryRequest r, string slug, DateTime now)
    {
        e.Slug = slug;
        e.Name = r.Name.Trim();
        e.Aliases = Clean(r.Aliases);
        e.Region = r.Region.Trim();
        e.Type = r.Type.Trim();
        e.GiName = Clean(r.GiName);
        e.Unesco = Clean(r.Unesco);
        e.Summary = r.Summary.Trim();
        e.History = Clean(r.History);
        e.Materials = Clean(r.Materials);
        e.Process = Clean(r.Process);
        e.Products = Clean(r.Products);
        e.Story = Clean(r.Story);
        e.Visit = Clean(r.Visit);
        e.Sources = Clean(r.Sources);
        e.DisplayOrder = r.DisplayOrder;
        e.IsActive = r.IsActive;
        e.UpdatedAt = now;
    }

    private static CraftHeritageEntryDto ToDto(CraftHeritageEntry e) => new()
    {
        Id = e.Id, Slug = e.Slug, Name = e.Name, Aliases = e.Aliases, Region = e.Region, Type = e.Type, GiName = e.GiName,
        Unesco = e.Unesco, Summary = e.Summary, History = e.History, Materials = e.Materials, Process = e.Process,
        Products = e.Products, Story = e.Story, Visit = e.Visit, Sources = e.Sources, DisplayOrder = e.DisplayOrder,
        IsActive = e.IsActive, CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt,
    };
}

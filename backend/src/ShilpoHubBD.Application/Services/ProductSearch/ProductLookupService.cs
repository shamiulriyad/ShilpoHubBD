using System.Text.RegularExpressions;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.ProductSearch;

/// <summary>Admin-managed product types (the "subcategory") and materials used to describe and filter products.</summary>
public class ProductLookupService : IProductLookupService
{
    private readonly IProductLookupRepository _repository;

    public ProductLookupService(IProductLookupRepository repository)
    {
        _repository = repository;
    }

    // ---- product types ----
    public async Task<List<LookupItemDto>> GetTypesAsync(bool includeInactive, CancellationToken cancellationToken)
        => (await _repository.GetTypesAsync(includeInactive, cancellationToken)).Select(ToDto).ToList();

    public async Task<LookupItemDto> CreateTypeAsync(SaveLookupItemRequest request, CancellationToken cancellationToken)
    {
        var slug = SlugFor(request);
        if (await _repository.TypeSlugExistsAsync(slug, null, cancellationToken))
        {
            throw new ConflictException("A product type with this slug already exists.");
        }

        var now = DateTime.UtcNow;
        var type = new ProductType { Id = Guid.NewGuid(), CreatedAt = now };
        Apply(type, request, slug, now);
        type.IsActive = true;
        await _repository.AddTypeAsync(type, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(type);
    }

    public async Task<LookupItemDto> UpdateTypeAsync(Guid id, SaveLookupItemRequest request, CancellationToken cancellationToken)
    {
        var type = await _repository.GetTypeAsync(id, cancellationToken) ?? throw new NotFoundException("Product type not found.");
        var slug = SlugFor(request);
        if (await _repository.TypeSlugExistsAsync(slug, id, cancellationToken))
        {
            throw new ConflictException("A product type with this slug already exists.");
        }

        Apply(type, request, slug, DateTime.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(type);
    }

    public async Task DeleteTypeAsync(Guid id, CancellationToken cancellationToken)
    {
        // Products keep working: the FK is SetNull, they just lose their type until reassigned.
        _repository.RemoveType(await _repository.GetTypeAsync(id, cancellationToken) ?? throw new NotFoundException("Product type not found."));
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- materials ----
    public async Task<List<LookupItemDto>> GetMaterialsAsync(bool includeInactive, CancellationToken cancellationToken)
        => (await _repository.GetMaterialsAsync(includeInactive, cancellationToken)).Select(ToDto).ToList();

    public async Task<LookupItemDto> CreateMaterialAsync(SaveLookupItemRequest request, CancellationToken cancellationToken)
    {
        var slug = SlugFor(request);
        if (await _repository.MaterialSlugExistsAsync(slug, null, cancellationToken))
        {
            throw new ConflictException("A material with this slug already exists.");
        }

        var now = DateTime.UtcNow;
        var material = new Material { Id = Guid.NewGuid(), CreatedAt = now };
        Apply(material, request, slug, now);
        material.IsActive = true;
        await _repository.AddMaterialAsync(material, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(material);
    }

    public async Task<LookupItemDto> UpdateMaterialAsync(Guid id, SaveLookupItemRequest request, CancellationToken cancellationToken)
    {
        var material = await _repository.GetMaterialAsync(id, cancellationToken) ?? throw new NotFoundException("Material not found.");
        var slug = SlugFor(request);
        if (await _repository.MaterialSlugExistsAsync(slug, id, cancellationToken))
        {
            throw new ConflictException("A material with this slug already exists.");
        }

        Apply(material, request, slug, DateTime.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(material);
    }

    public async Task DeleteMaterialAsync(Guid id, CancellationToken cancellationToken)
    {
        var material = await _repository.GetMaterialAsync(id, cancellationToken) ?? throw new NotFoundException("Material not found.");
        if (await _repository.MaterialInUseAsync(id, cancellationToken))
        {
            throw new ConflictException("This material is used by products. Deactivate it instead of deleting it.");
        }

        _repository.RemoveMaterial(material);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ----
    public static string Slugify(string value)
        => Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

    private static string SlugFor(SaveLookupItemRequest request)
    {
        var slug = Slugify(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug);
        return slug.Length == 0 ? throw new ConflictException("A slug could not be generated; give the item an English name or a slug.") : slug;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Apply(ProductType type, SaveLookupItemRequest r, string slug, DateTime now)
    {
        type.Name = r.Name.Trim();
        type.NameBn = Clean(r.NameBn);
        type.Slug = slug;
        type.DisplayOrder = r.DisplayOrder;
        type.IsActive = r.IsActive;
        type.UpdatedAt = now;
    }

    private static void Apply(Material material, SaveLookupItemRequest r, string slug, DateTime now)
    {
        material.Name = r.Name.Trim();
        material.NameBn = Clean(r.NameBn);
        material.Slug = slug;
        material.DisplayOrder = r.DisplayOrder;
        material.IsActive = r.IsActive;
        material.UpdatedAt = now;
    }

    private static LookupItemDto ToDto(ProductType t) => new() { Id = t.Id, Name = t.Name, NameBn = t.NameBn, Slug = t.Slug, DisplayOrder = t.DisplayOrder, IsActive = t.IsActive };

    private static LookupItemDto ToDto(Material m) => new() { Id = m.Id, Name = m.Name, NameBn = m.NameBn, Slug = m.Slug, DisplayOrder = m.DisplayOrder, IsActive = m.IsActive };
}

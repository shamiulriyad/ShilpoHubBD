using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Marketplace;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(includeInactive, cancellationToken);
        var counts = await _categoryRepository.GetActiveProductCountsAsync(cancellationToken);
        return categories.Select(c => ToDto(c, counts.GetValueOrDefault(c.Id))).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var counts = await _categoryRepository.GetActiveProductCountsAsync(cancellationToken);
        return ToDto(category, counts.GetValueOrDefault(category.Id));
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken);
        var now = DateTime.UtcNow;

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        return ToDto(category, 0);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (!category.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            category.Slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken);
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.ImageUrl = request.ImageUrl?.Trim();
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.SaveChangesAsync(cancellationToken);

        var counts = await _categoryRepository.GetActiveProductCountsAsync(cancellationToken);
        return ToDto(category, counts.GetValueOrDefault(category.Id));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (await _categoryRepository.HasProductsAsync(id, cancellationToken))
        {
            throw new ConflictException("Cannot delete a category that has products. Deactivate it instead.");
        }

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _categoryRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static CategoryDto ToDto(Category category, int productCount) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug,
        Description = category.Description,
        ImageUrl = category.ImageUrl,
        DisplayOrder = category.DisplayOrder,
        IsActive = category.IsActive,
        ProductCount = productCount,
    };
}

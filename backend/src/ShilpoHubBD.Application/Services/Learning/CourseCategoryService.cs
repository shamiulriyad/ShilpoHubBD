using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class CourseCategoryService : ICourseCategoryService
{
    private readonly ICourseCategoryRepository _categoryRepository;

    public CourseCategoryService(ICourseCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CourseCategoryDto>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(activeOnly, cancellationToken);
        return categories.Select(ToDto).ToList();
    }

    public async Task<CourseCategoryDto> CreateAsync(CreateCourseCategoryRequest request, CancellationToken cancellationToken)
    {
        if (await _categoryRepository.ExistsByNameAsync(request.Name.Trim(), cancellationToken))
        {
            throw new ConflictException("A course category with this name already exists.");
        }

        var category = new CourseCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    private static CourseCategoryDto ToDto(CourseCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        IsActive = category.IsActive,
        CreatedAt = category.CreatedAt,
    };
}

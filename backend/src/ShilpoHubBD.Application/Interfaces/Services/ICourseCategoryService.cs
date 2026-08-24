using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICourseCategoryService
{
    Task<List<CourseCategoryDto>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<CourseCategoryDto> CreateAsync(CreateCourseCategoryRequest request, CancellationToken cancellationToken);
}

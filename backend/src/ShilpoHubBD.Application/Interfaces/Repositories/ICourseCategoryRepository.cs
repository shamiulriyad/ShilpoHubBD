using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICourseCategoryRepository
{
    Task<List<CourseCategory>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<CourseCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
    Task AddAsync(CourseCategory category, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

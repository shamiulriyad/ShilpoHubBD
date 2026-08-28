using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Repositories;

public class CourseCategoryRepository : ICourseCategoryRepository
{
    private readonly ShilpoHubDbContext _context;

    public CourseCategoryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<CourseCategory>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var query = _context.CourseCategories.AsQueryable();
        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        return query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public Task<CourseCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.CourseCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        => _context.CourseCategories.AnyAsync(c => EF.Functions.ILike(c.Name, name), cancellationToken);

    public async Task AddAsync(CourseCategory category, CancellationToken cancellationToken)
        => await _context.CourseCategories.AddAsync(category, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

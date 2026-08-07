using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ShilpoHubDbContext _context;

    public CategoryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<Category>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
        => _context.Categories
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
        => _context.Categories.AnyAsync(c => c.Slug == slug, cancellationToken);

    public Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken)
        => _context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);

    public async Task<Dictionary<Guid, int>> GetActiveProductCountsAsync(CancellationToken cancellationToken)
        => await _context.Products
            .Where(p => p.IsActive)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken)
        => await _context.Categories.AddAsync(category, cancellationToken);

    public void Remove(Category category)
        => _context.Categories.Remove(category);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

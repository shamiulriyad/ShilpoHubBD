using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageSkillRepository : IHeritageSkillRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageSkillRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<HeritageSkill>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var query = _context.HeritageSkills.AsQueryable();
        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        return query.OrderBy(s => s.Name).ToListAsync(cancellationToken);
    }

    public Task<HeritageSkill?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.HeritageSkills.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        => _context.HeritageSkills.AnyAsync(s => EF.Functions.ILike(s.Name, name), cancellationToken);

    public async Task AddAsync(HeritageSkill skill, CancellationToken cancellationToken)
        => await _context.HeritageSkills.AddAsync(skill, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

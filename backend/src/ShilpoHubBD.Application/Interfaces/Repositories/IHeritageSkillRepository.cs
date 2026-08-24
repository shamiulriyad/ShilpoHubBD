using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritageSkillRepository
{
    Task<List<HeritageSkill>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<HeritageSkill?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
    Task AddAsync(HeritageSkill skill, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

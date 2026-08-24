using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAcademyMemberProfileRepository
{
    Task<AcademyMemberProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AcademyMemberProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(AcademyMemberProfile profile, CancellationToken cancellationToken);

    Task<AcademyMemberSkill?> GetSkillAsync(Guid profileId, Guid heritageSkillId, CancellationToken cancellationToken);
    Task AddSkillAsync(AcademyMemberSkill skill, CancellationToken cancellationToken);
    void RemoveSkill(AcademyMemberSkill skill);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

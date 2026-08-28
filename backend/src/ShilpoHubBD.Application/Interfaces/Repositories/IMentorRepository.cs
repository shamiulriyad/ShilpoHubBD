using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IMentorRepository
{
    Task<MentorProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<MentorProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<(List<MentorProfile> Items, int TotalCount)> GetPagedAsync(bool activeOnly, int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(MentorProfile mentor, CancellationToken cancellationToken);

    Task<MentorSkill?> GetSkillAsync(Guid mentorProfileId, Guid heritageSkillId, CancellationToken cancellationToken);
    Task AddSkillAsync(MentorSkill skill, CancellationToken cancellationToken);
    void RemoveSkill(MentorSkill skill);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

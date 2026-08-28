using ShilpoHubBD.Domain.Entities.SkillAssessment;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ISkillAssessmentRepository
{
    Task<SkillAssessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<SkillAssessment>> GetByProfileAsync(Guid academyMemberProfileId, CancellationToken cancellationToken);
    Task AddAsync(SkillAssessment assessment, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

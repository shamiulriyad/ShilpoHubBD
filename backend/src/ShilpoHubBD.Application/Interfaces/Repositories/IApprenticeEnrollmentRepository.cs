using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IApprenticeEnrollmentRepository
{
    Task<ApprenticeEnrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApprenticeEnrollment?> GetByProgramAndApprenticeAsync(Guid programId, Guid apprenticeUserId, CancellationToken cancellationToken);
    Task<List<ApprenticeEnrollment>> GetByApprenticeAsync(Guid apprenticeUserId, CancellationToken cancellationToken);
    Task<List<ApprenticeEnrollment>> GetByProgramAsync(Guid programId, CancellationToken cancellationToken);
    Task<int> GetActiveCountByProgramAsync(Guid programId, CancellationToken cancellationToken);
    Task AddAsync(ApprenticeEnrollment enrollment, CancellationToken cancellationToken);
    Task AddMilestoneProgressAsync(ApprenticeMilestoneProgress progress, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

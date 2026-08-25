using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IApprenticeshipProgramRepository
{
    Task<ApprenticeshipProgram?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<ApprenticeshipProgram> Items, int TotalCount)> GetPagedAsync(
        ApprenticeshipProgramQueryParameters query, CancellationToken cancellationToken);
    Task<List<ApprenticeshipProgram>> GetByMentorAsync(Guid mentorId, CancellationToken cancellationToken);
    Task<List<ApprenticeshipProgram>> GetByTrainerProfileAsync(Guid trainerProfileId, CancellationToken cancellationToken);
    Task AddAsync(ApprenticeshipProgram program, CancellationToken cancellationToken);
    void Remove(ApprenticeshipProgram program);

    Task<TrainingMilestone?> GetMilestoneByIdAsync(Guid milestoneId, CancellationToken cancellationToken);
    Task AddMilestoneAsync(TrainingMilestone milestone, CancellationToken cancellationToken);
    void RemoveMilestone(TrainingMilestone milestone);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IApprenticeshipProgramService
{
    Task<ApprenticeshipProgramDto> CreateAsync(Guid userId, CreateApprenticeshipProgramRequest request, CancellationToken cancellationToken);

    Task<ApprenticeshipProgramDto> UpdateAsync(Guid userId, Guid programId, UpdateApprenticeshipProgramRequest request, CancellationToken cancellationToken);

    Task<ApprenticeshipProgramDto> PublishAsync(Guid userId, Guid programId, CancellationToken cancellationToken);

    Task<ApprenticeshipProgramDto> CloseAsync(Guid userId, Guid programId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid programId, CancellationToken cancellationToken);

    Task<ApprenticeshipProgramDto> GetByIdAsync(Guid programId, Guid? currentUserId, CancellationToken cancellationToken);

    Task<PagedResult<ApprenticeshipProgramListItemDto>> GetPublishedAsync(ApprenticeshipProgramQueryParameters query, CancellationToken cancellationToken);

    Task<List<ApprenticeshipProgramListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken);

    Task<TrainingMilestoneDto> AddMilestoneAsync(Guid userId, Guid programId, CreateTrainingMilestoneRequest request, CancellationToken cancellationToken);

    Task<TrainingMilestoneDto> UpdateMilestoneAsync(Guid userId, Guid programId, Guid milestoneId, UpdateTrainingMilestoneRequest request, CancellationToken cancellationToken);

    Task DeleteMilestoneAsync(Guid userId, Guid programId, Guid milestoneId, CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Apprenticeship;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IApprenticeEnrollmentService
{
    Task<List<ApprenticeEnrollmentListItemDto>> GetMyEnrollmentsAsync(Guid apprenticeUserId, CancellationToken cancellationToken);

    Task<ApprenticeEnrollmentDto> GetByIdAsync(Guid userId, bool isAdmin, Guid enrollmentId, CancellationToken cancellationToken);

    Task<List<ApprenticeEnrollmentListItemDto>> GetByProgramAsync(Guid providerUserId, Guid programId, CancellationToken cancellationToken);

    Task<ApprenticeEnrollmentDto> UpdateMilestoneProgressAsync(
        Guid providerUserId, Guid enrollmentId, Guid milestoneId, UpdateMilestoneProgressRequest request, CancellationToken cancellationToken);

    Task<ApprenticeEnrollmentDto> CompleteAsync(Guid providerUserId, Guid enrollmentId, CancellationToken cancellationToken);
}

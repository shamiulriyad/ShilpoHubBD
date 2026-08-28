using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IEnrollmentService
{
    Task<CourseEnrollmentDto> EnrollAsync(Guid apprenticeUserId, Guid courseId, CancellationToken cancellationToken);
    Task<List<EnrollmentListItemDto>> GetMyEnrollmentsAsync(Guid apprenticeUserId, CancellationToken cancellationToken);
    Task<CourseEnrollmentDto> GetEnrollmentAsync(Guid userId, bool isAdmin, Guid enrollmentId, CancellationToken cancellationToken);
    Task<List<EnrollmentListItemDto>> GetByCourseAsync(Guid mentorUserId, Guid courseId, CancellationToken cancellationToken);
    Task<CourseEnrollmentDto> MarkLessonProgressAsync(
        Guid mentorUserId, Guid enrollmentId, MarkLessonProgressRequest request, CancellationToken cancellationToken);
    Task<CourseEnrollmentDto> CompleteEnrollmentAsync(Guid mentorUserId, Guid enrollmentId, CancellationToken cancellationToken);
}

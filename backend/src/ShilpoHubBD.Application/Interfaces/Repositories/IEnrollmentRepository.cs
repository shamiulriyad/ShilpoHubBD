using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IEnrollmentRepository
{
    Task<CourseEnrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CourseEnrollment?> GetByCourseAndApprenticeAsync(Guid courseId, Guid apprenticeId, CancellationToken cancellationToken);
    Task<List<CourseEnrollment>> GetByApprenticeAsync(Guid apprenticeId, CancellationToken cancellationToken);
    Task<List<CourseEnrollment>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task<int> GetActiveCountByCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task<int> GetCompletedCountByMentorAsync(Guid mentorId, CancellationToken cancellationToken);
    Task AddAsync(CourseEnrollment enrollment, CancellationToken cancellationToken);
    Task AddLessonProgressAsync(LessonProgress progress, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

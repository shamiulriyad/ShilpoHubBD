using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ShilpoHubDbContext _context;

    public EnrollmentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<CourseEnrollment> WithDetails()
        => _context.CourseEnrollments
            .Include(e => e.Course).ThenInclude(c => c.Lessons)
            .Include(e => e.Apprentice)
            .Include(e => e.LessonProgress).ThenInclude(p => p.Lesson)
            .AsSplitQuery();

    public Task<CourseEnrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<CourseEnrollment?> GetByCourseAndApprenticeAsync(Guid courseId, Guid apprenticeId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(e => e.CourseId == courseId && e.ApprenticeId == apprenticeId, cancellationToken);

    public Task<List<CourseEnrollment>> GetByApprenticeAsync(Guid apprenticeId, CancellationToken cancellationToken)
        => WithDetails().Where(e => e.ApprenticeId == apprenticeId).OrderByDescending(e => e.EnrolledAt).ToListAsync(cancellationToken);

    public Task<List<CourseEnrollment>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken)
        => WithDetails().Where(e => e.CourseId == courseId).OrderByDescending(e => e.EnrolledAt).ToListAsync(cancellationToken);

    public Task<int> GetActiveCountByCourseAsync(Guid courseId, CancellationToken cancellationToken)
        => _context.CourseEnrollments.CountAsync(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active, cancellationToken);

    public Task<int> GetCompletedCountByMentorAsync(Guid mentorId, CancellationToken cancellationToken)
        => _context.CourseEnrollments.CountAsync(
            e => e.Course.MentorId == mentorId && e.Status == EnrollmentStatus.Completed, cancellationToken);

    public async Task AddAsync(CourseEnrollment enrollment, CancellationToken cancellationToken)
        => await _context.CourseEnrollments.AddAsync(enrollment, cancellationToken);

    public async Task AddLessonProgressAsync(LessonProgress progress, CancellationToken cancellationToken)
        => await _context.LessonProgress.AddAsync(progress, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

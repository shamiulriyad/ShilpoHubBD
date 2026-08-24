using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ITrainingCertificateRepository _trainingCertificateRepository;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        ICourseRepository courseRepository,
        ITrainingCertificateRepository trainingCertificateRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _trainingCertificateRepository = trainingCertificateRepository;
    }

    public async Task<CourseEnrollmentDto> EnrollAsync(Guid apprenticeUserId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        if (course.Status != CourseStatus.Published)
        {
            throw new ConflictException("You can only enroll in published courses.");
        }

        if (AuthorUserId(course) == apprenticeUserId)
        {
            throw new ConflictException("You cannot enroll in your own course.");
        }

        var existing = await _enrollmentRepository.GetByCourseAndApprenticeAsync(courseId, apprenticeUserId, cancellationToken);
        if (existing is not null)
        {
            return ToDto(existing);
        }

        if (course.MaxApprentices.HasValue)
        {
            var activeCount = await _enrollmentRepository.GetActiveCountByCourseAsync(courseId, cancellationToken);
            if (activeCount >= course.MaxApprentices.Value)
            {
                throw new ConflictException("This course has reached its maximum number of apprentices.");
            }
        }

        var enrollment = new CourseEnrollment
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            ApprenticeId = apprenticeUserId,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow,
        };

        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await _enrollmentRepository.SaveChangesAsync(cancellationToken);

        var created = await _enrollmentRepository.GetByIdAsync(enrollment.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<List<EnrollmentListItemDto>> GetMyEnrollmentsAsync(Guid apprenticeUserId, CancellationToken cancellationToken)
    {
        var enrollments = await _enrollmentRepository.GetByApprenticeAsync(apprenticeUserId, cancellationToken);
        return enrollments.Select(ToListItemDto).ToList();
    }

    public async Task<CourseEnrollmentDto> GetEnrollmentAsync(Guid userId, bool isAdmin, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("Enrollment not found.");

        if (!isAdmin && enrollment.ApprenticeId != userId && AuthorUserId(enrollment.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this enrollment.");
        }

        return ToDto(enrollment);
    }

    public async Task<List<EnrollmentListItemDto>> GetByCourseAsync(Guid mentorUserId, Guid courseId, CancellationToken cancellationToken)
    {
        await GetOwnedCourseAsync(mentorUserId, courseId, cancellationToken);

        var enrollments = await _enrollmentRepository.GetByCourseAsync(courseId, cancellationToken);
        return enrollments.Select(ToListItemDto).ToList();
    }

    public async Task<CourseEnrollmentDto> MarkLessonProgressAsync(
        Guid mentorUserId, Guid enrollmentId, MarkLessonProgressRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await GetEnrollmentOwnedByMentorAsync(mentorUserId, enrollmentId, cancellationToken);

        if (!enrollment.Course.Lessons.Any(l => l.Id == request.LessonId))
        {
            throw new NotFoundException("Lesson not found in this course.");
        }

        var progress = enrollment.LessonProgress.FirstOrDefault(p => p.LessonId == request.LessonId);
        if (progress is null)
        {
            progress = new LessonProgress
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                LessonId = request.LessonId,
                IsCompleted = request.IsCompleted,
                CompletedAt = request.IsCompleted ? DateTime.UtcNow : null,
            };
            await _enrollmentRepository.AddLessonProgressAsync(progress, cancellationToken);
            enrollment.LessonProgress.Add(progress);
        }
        else
        {
            progress.IsCompleted = request.IsCompleted;
            progress.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;
        }

        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
        return ToDto(enrollment);
    }

    public async Task<CourseEnrollmentDto> CompleteEnrollmentAsync(Guid mentorUserId, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await GetEnrollmentOwnedByMentorAsync(mentorUserId, enrollmentId, cancellationToken);

        if (enrollment.Status == EnrollmentStatus.Completed)
        {
            throw new ConflictException("This enrollment has already been marked complete.");
        }

        var now = DateTime.UtcNow;
        enrollment.Status = EnrollmentStatus.Completed;
        enrollment.CompletedAt = now;

        var existingCertificate = await _trainingCertificateRepository.GetByEnrollmentIdAsync(enrollment.Id, cancellationToken);
        if (existingCertificate is null)
        {
            var certificate = new TrainingCertificate
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                CertificateNumber = GenerateCertificateNumber(now),
                CourseTitle = enrollment.Course.Title,
                ApprenticeName = enrollment.Apprentice.FullName,
                MentorName = AuthorNameOf(enrollment.Course),
                IsRevoked = false,
                IssuedAt = now,
            };

            await _trainingCertificateRepository.AddAsync(certificate, cancellationToken);
        }

        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
        return ToDto(enrollment);
    }

    private async Task<Domain.Entities.Learning.Course> GetOwnedCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        if (AuthorUserId(course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this course.");
        }

        return course;
    }

    private async Task<CourseEnrollment> GetEnrollmentOwnedByMentorAsync(Guid mentorUserId, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("Enrollment not found.");

        if (AuthorUserId(enrollment.Course) != mentorUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this enrollment.");
        }

        return enrollment;
    }

    private static Guid? AuthorUserId(Domain.Entities.Learning.Course course)
        => course.Mentor?.UserId ?? course.TrainerProfile?.UserId;

    private static string AuthorNameOf(Domain.Entities.Learning.Course course)
        => course.Mentor?.User.FullName ?? course.TrainerProfile?.User.FullName ?? string.Empty;

    private static string GenerateCertificateNumber(DateTime issuedAt)
        => $"SH-TRN-{issuedAt:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    private static EnrollmentListItemDto ToListItemDto(CourseEnrollment enrollment) => new()
    {
        Id = enrollment.Id,
        CourseId = enrollment.CourseId,
        CourseTitle = enrollment.Course.Title,
        ApprenticeId = enrollment.ApprenticeId,
        ApprenticeName = enrollment.Apprentice.FullName,
        Status = enrollment.Status.ToString(),
        EnrolledAt = enrollment.EnrolledAt,
        ProgressPercent = CalculateProgressPercent(enrollment),
    };

    private static CourseEnrollmentDto ToDto(CourseEnrollment enrollment)
    {
        var totalLessons = enrollment.Course.Lessons.Count;
        var completedLessons = enrollment.LessonProgress.Count(p => p.IsCompleted);

        return new CourseEnrollmentDto
        {
            Id = enrollment.Id,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course.Title,
            ApprenticeId = enrollment.ApprenticeId,
            ApprenticeName = enrollment.Apprentice.FullName,
            Status = enrollment.Status.ToString(),
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt,
            TotalLessons = totalLessons,
            CompletedLessons = completedLessons,
            ProgressPercent = CalculateProgressPercent(enrollment),
            LessonProgress = enrollment.Course.Lessons
                .OrderBy(l => l.DisplayOrder)
                .Select(l =>
                {
                    var progress = enrollment.LessonProgress.FirstOrDefault(p => p.LessonId == l.Id);
                    return new LessonProgressDto
                    {
                        LessonId = l.Id,
                        LessonTitle = l.Title,
                        IsCompleted = progress?.IsCompleted ?? false,
                        CompletedAt = progress?.CompletedAt,
                    };
                })
                .ToList(),
        };
    }

    private static decimal CalculateProgressPercent(CourseEnrollment enrollment)
    {
        var totalLessons = enrollment.Course.Lessons.Count;
        if (totalLessons == 0)
        {
            return 0;
        }

        var completedLessons = enrollment.LessonProgress.Count(p => p.IsCompleted);
        return Math.Round(completedLessons / (decimal)totalLessons * 100m, 1);
    }
}

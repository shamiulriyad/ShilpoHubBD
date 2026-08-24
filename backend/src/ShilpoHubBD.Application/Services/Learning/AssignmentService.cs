using ShilpoHubBD.Application.DTOs.Assessment;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Assessment;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public AssignmentService(
        IAssignmentRepository assignmentRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<AssignmentDto> CreateAsync(Guid userId, Guid courseId, CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var now = DateTime.UtcNow;
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            MaxScore = request.MaxScore,
            DueAt = request.DueAt,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _assignmentRepository.AddAsync(assignment, cancellationToken);
        await _assignmentRepository.SaveChangesAsync(cancellationToken);

        var created = await _assignmentRepository.GetByIdAsync(assignment.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<AssignmentDto> UpdateAsync(Guid userId, Guid assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var assignment = await GetOwnedAssignmentAsync(userId, assignmentId, cancellationToken);

        assignment.Title = request.Title.Trim();
        assignment.Description = request.Description.Trim();
        assignment.MaxScore = request.MaxScore;
        assignment.DueAt = request.DueAt;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.SaveChangesAsync(cancellationToken);
        return ToDto(assignment);
    }

    public async Task DeleteAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken)
    {
        var assignment = await GetOwnedAssignmentAsync(userId, assignmentId, cancellationToken);

        if (assignment.Submissions.Count > 0)
        {
            throw new ConflictException("This assignment already has submissions and cannot be deleted.");
        }

        _assignmentRepository.Remove(assignment);
        await _assignmentRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AssignmentDto> GetByIdAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken)
            ?? throw new NotFoundException("Assignment not found.");

        await EnsureCanViewCourseAsync(userId, assignment.Course, cancellationToken);
        return ToDto(assignment);
    }

    public async Task<List<AssignmentListItemDto>> GetByCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        await EnsureCanViewCourseAsync(userId, course, cancellationToken);

        var assignments = await _assignmentRepository.GetByCourseAsync(courseId, cancellationToken);
        return assignments.Select(ToListItemDto).ToList();
    }

    public async Task<AssignmentSubmissionDto> SubmitAsync(
        Guid studentUserId, Guid assignmentId, SubmitAssignmentRequest request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken)
            ?? throw new NotFoundException("Assignment not found.");

        await EnsureEnrolledAsync(studentUserId, assignment.CourseId, cancellationToken);

        var existing = await _assignmentRepository.GetSubmissionByStudentAsync(assignmentId, studentUserId, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == SubmissionStatus.Graded)
            {
                throw new ConflictException("This assignment has already been graded and cannot be resubmitted.");
            }

            existing.SubmissionText = request.SubmissionText.Trim();
            existing.AttachmentUrl = request.AttachmentUrl?.Trim();
            existing.SubmittedAt = DateTime.UtcNow;

            await _assignmentRepository.SaveChangesAsync(cancellationToken);
            return ToSubmissionDto(existing);
        }

        var submission = new AssignmentSubmission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            StudentUserId = studentUserId,
            SubmissionText = request.SubmissionText.Trim(),
            AttachmentUrl = request.AttachmentUrl?.Trim(),
            Status = SubmissionStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
        };

        await _assignmentRepository.AddSubmissionAsync(submission, cancellationToken);
        await _assignmentRepository.SaveChangesAsync(cancellationToken);

        var created = await _assignmentRepository.GetSubmissionByIdAsync(submission.Id, cancellationToken);
        return ToSubmissionDto(created!);
    }

    public async Task<AssignmentSubmissionDto> GetMySubmissionAsync(Guid studentUserId, Guid assignmentId, CancellationToken cancellationToken)
    {
        var submission = await _assignmentRepository.GetSubmissionByStudentAsync(assignmentId, studentUserId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");

        var full = await _assignmentRepository.GetSubmissionByIdAsync(submission.Id, cancellationToken);
        return ToSubmissionDto(full!);
    }

    public async Task<List<AssignmentSubmissionDto>> GetSubmissionsAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken)
    {
        var assignment = await GetOwnedAssignmentAsync(userId, assignmentId, cancellationToken);
        return assignment.Submissions.OrderByDescending(s => s.SubmittedAt).Select(ToSubmissionDto).ToList();
    }

    public async Task<AssignmentSubmissionDto> GradeAsync(
        Guid userId, Guid submissionId, GradeAssignmentSubmissionRequest request, CancellationToken cancellationToken)
    {
        var submission = await _assignmentRepository.GetSubmissionByIdAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");

        if (AuthorUserId(submission.Assignment.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to grade this submission.");
        }

        if (request.Score < 0 || request.Score > submission.Assignment.MaxScore)
        {
            throw new ConflictException($"Score must be between 0 and {submission.Assignment.MaxScore}.");
        }

        submission.Score = request.Score;
        submission.Feedback = request.Feedback?.Trim();
        submission.Status = SubmissionStatus.Graded;
        submission.GradedAt = DateTime.UtcNow;
        submission.GradedByUserId = userId;

        await _assignmentRepository.SaveChangesAsync(cancellationToken);
        return ToSubmissionDto(submission);
    }

    private async Task<Course> GetOwnedCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        if (AuthorUserId(course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this course.");
        }

        return course;
    }

    private async Task<Assignment> GetOwnedAssignmentAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken)
            ?? throw new NotFoundException("Assignment not found.");

        if (AuthorUserId(assignment.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this assignment.");
        }

        return assignment;
    }

    private async Task EnsureEnrolledAsync(Guid studentUserId, Guid courseId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByCourseAndApprenticeAsync(courseId, studentUserId, cancellationToken);
        if (enrollment is null || enrollment.Status != EnrollmentStatus.Active)
        {
            throw new ConflictException("You must be actively enrolled in this course.");
        }
    }

    private async Task EnsureCanViewCourseAsync(Guid userId, Course course, CancellationToken cancellationToken)
    {
        if (AuthorUserId(course) == userId)
        {
            return;
        }

        var enrollment = await _enrollmentRepository.GetByCourseAndApprenticeAsync(course.Id, userId, cancellationToken);
        if (enrollment is null)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this course's assignments.");
        }
    }

    private static Guid? AuthorUserId(Course course)
        => course.Mentor?.UserId ?? course.TrainerProfile?.UserId;

    private static AssignmentListItemDto ToListItemDto(Assignment assignment) => new()
    {
        Id = assignment.Id,
        CourseId = assignment.CourseId,
        CourseTitle = assignment.Course.Title,
        Title = assignment.Title,
        MaxScore = assignment.MaxScore,
        DueAt = assignment.DueAt,
        SubmissionCount = assignment.Submissions.Count,
    };

    private static AssignmentDto ToDto(Assignment assignment) => new()
    {
        Id = assignment.Id,
        CourseId = assignment.CourseId,
        CourseTitle = assignment.Course.Title,
        Title = assignment.Title,
        Description = assignment.Description,
        MaxScore = assignment.MaxScore,
        DueAt = assignment.DueAt,
        SubmissionCount = assignment.Submissions.Count,
        CreatedAt = assignment.CreatedAt,
        UpdatedAt = assignment.UpdatedAt,
    };

    private static AssignmentSubmissionDto ToSubmissionDto(AssignmentSubmission submission) => new()
    {
        Id = submission.Id,
        AssignmentId = submission.AssignmentId,
        AssignmentTitle = submission.Assignment.Title,
        MaxScore = submission.Assignment.MaxScore,
        StudentUserId = submission.StudentUserId,
        StudentName = submission.Student.FullName,
        SubmissionText = submission.SubmissionText,
        AttachmentUrl = submission.AttachmentUrl,
        Status = submission.Status.ToString(),
        Score = submission.Score,
        Feedback = submission.Feedback,
        SubmittedAt = submission.SubmittedAt,
        GradedAt = submission.GradedAt,
    };
}

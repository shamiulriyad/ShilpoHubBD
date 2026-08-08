using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMentorRepository _mentorRepository;

    public CourseService(ICourseRepository courseRepository, IMentorRepository mentorRepository)
    {
        _courseRepository = courseRepository;
        _mentorRepository = mentorRepository;
    }

    public async Task<CourseDto> CreateAsync(Guid userId, CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var mentor = await RequireMentorAsync(userId, cancellationToken);

        var now = DateTime.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            MentorId = mentor.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            Status = CourseStatus.Draft,
            MaxApprentices = request.MaxApprentices,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _courseRepository.AddAsync(course, cancellationToken);
        await _courseRepository.SaveChangesAsync(cancellationToken);

        var created = await _courseRepository.GetByIdAsync(course.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<CourseDto> UpdateAsync(Guid userId, Guid courseId, UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        course.Title = request.Title.Trim();
        course.Description = request.Description.Trim();
        course.Category = request.Category.Trim();
        course.MaxApprentices = request.MaxApprentices;
        course.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.SaveChangesAsync(cancellationToken);
        return ToDto(course);
    }

    public async Task<CourseDto> PublishAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        if (course.Status == CourseStatus.Published)
        {
            throw new ConflictException("This course is already published.");
        }

        if (course.Lessons.Count == 0)
        {
            throw new ConflictException("Add at least one lesson before publishing this course.");
        }

        var now = DateTime.UtcNow;
        course.Status = CourseStatus.Published;
        course.PublishedAt = now;
        course.UpdatedAt = now;

        await _courseRepository.SaveChangesAsync(cancellationToken);
        return ToDto(course);
    }

    public async Task<CourseDto> ArchiveAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        if (course.Status == CourseStatus.Archived)
        {
            throw new ConflictException("This course is already archived.");
        }

        course.Status = CourseStatus.Archived;
        course.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.SaveChangesAsync(cancellationToken);
        return ToDto(course);
    }

    public async Task<CourseDto> GetByIdAsync(Guid courseId, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        if (course.Status != CourseStatus.Published && course.Mentor.UserId != currentUserId)
        {
            throw new NotFoundException("Course not found.");
        }

        return ToDto(course);
    }

    public async Task<PagedResult<CourseListItemDto>> GetPublishedAsync(CourseQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _courseRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<CourseListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<List<CourseListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var mentor = await RequireMentorAsync(userId, cancellationToken);
        var courses = await _courseRepository.GetByMentorAsync(mentor.Id, cancellationToken);
        return courses.Select(ToListItemDto).ToList();
    }

    public async Task<CourseLessonDto> AddLessonAsync(Guid userId, Guid courseId, CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var now = DateTime.UtcNow;
        var lesson = new CourseLesson
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            VideoUrl = request.VideoUrl,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _courseRepository.AddLessonAsync(lesson, cancellationToken);
        course.UpdatedAt = now;
        await _courseRepository.SaveChangesAsync(cancellationToken);

        return ToLessonDto(lesson);
    }

    public async Task<CourseLessonDto> UpdateLessonAsync(
        Guid userId, Guid courseId, Guid lessonId, UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var lesson = await _courseRepository.GetLessonByIdAsync(lessonId, cancellationToken)
            ?? throw new NotFoundException("Lesson not found.");

        if (lesson.CourseId != courseId)
        {
            throw new NotFoundException("Lesson not found.");
        }

        lesson.Title = request.Title.Trim();
        lesson.Content = request.Content.Trim();
        lesson.VideoUrl = request.VideoUrl;
        lesson.DisplayOrder = request.DisplayOrder;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.SaveChangesAsync(cancellationToken);
        return ToLessonDto(lesson);
    }

    public async Task DeleteLessonAsync(Guid userId, Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var lesson = await _courseRepository.GetLessonByIdAsync(lessonId, cancellationToken)
            ?? throw new NotFoundException("Lesson not found.");

        if (lesson.CourseId != courseId)
        {
            throw new NotFoundException("Lesson not found.");
        }

        _courseRepository.RemoveLesson(lesson);
        await _courseRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Domain.Entities.Learning.MentorProfile> RequireMentorAsync(Guid userId, CancellationToken cancellationToken)
        => await _mentorRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ConflictException("You must have a mentor profile before managing courses.");

    private async Task<Course> GetOwnedCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        if (course.Mentor.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this course.");
        }

        return course;
    }

    private static CourseListItemDto ToListItemDto(Course course) => new()
    {
        Id = course.Id,
        MentorName = course.Mentor.User.FullName,
        Title = course.Title,
        Category = course.Category,
        Status = course.Status.ToString(),
        LessonCount = course.Lessons.Count,
        MaxApprentices = course.MaxApprentices,
        ActiveEnrollmentCount = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
    };

    private static CourseDto ToDto(Course course) => new()
    {
        Id = course.Id,
        MentorId = course.MentorId,
        MentorName = course.Mentor.User.FullName,
        Title = course.Title,
        Description = course.Description,
        Category = course.Category,
        Status = course.Status.ToString(),
        MaxApprentices = course.MaxApprentices,
        ActiveEnrollmentCount = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
        Lessons = course.Lessons.OrderBy(l => l.DisplayOrder).Select(ToLessonDto).ToList(),
        CreatedAt = course.CreatedAt,
        UpdatedAt = course.UpdatedAt,
        PublishedAt = course.PublishedAt,
    };

    private static CourseLessonDto ToLessonDto(CourseLesson lesson) => new()
    {
        Id = lesson.Id,
        CourseId = lesson.CourseId,
        Title = lesson.Title,
        Content = lesson.Content,
        VideoUrl = lesson.VideoUrl,
        DisplayOrder = lesson.DisplayOrder,
    };
}

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
    private readonly IAcademyMemberProfileRepository _academyMemberProfileRepository;
    private readonly ICourseCategoryRepository _categoryRepository;

    public CourseService(
        ICourseRepository courseRepository,
        IMentorRepository mentorRepository,
        IAcademyMemberProfileRepository academyMemberProfileRepository,
        ICourseCategoryRepository categoryRepository)
    {
        _courseRepository = courseRepository;
        _mentorRepository = mentorRepository;
        _academyMemberProfileRepository = academyMemberProfileRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CourseDto> CreateAsync(Guid userId, CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var (mentor, trainer) = await ResolveAuthorAsync(userId, cancellationToken);
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var now = DateTime.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            MentorId = mentor?.Id,
            TrainerProfileId = trainer?.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            CategoryId = request.CategoryId,
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
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        course.Title = request.Title.Trim();
        course.Description = request.Description.Trim();
        course.Category = request.Category.Trim();
        course.CategoryId = request.CategoryId;
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

    public async Task DeleteAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        if (course.Status != CourseStatus.Draft)
        {
            throw new ConflictException("Only draft courses can be deleted. Archive it instead.");
        }

        _courseRepository.Remove(course);
        await _courseRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<CourseDto> GetByIdAsync(Guid courseId, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        var isOwner = course.Mentor?.UserId == currentUserId || course.TrainerProfile?.UserId == currentUserId;
        if (course.Status != CourseStatus.Published && !isOwner)
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
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken);
        if (mentor is not null)
        {
            var mentorCourses = await _courseRepository.GetByMentorAsync(mentor.Id, cancellationToken);
            return mentorCourses.Select(ToListItemDto).ToList();
        }

        var trainerProfile = await _academyMemberProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (trainerProfile is not null && trainerProfile.Role == AcademyMemberRole.Trainer)
        {
            var trainerCourses = await _courseRepository.GetByTrainerProfileAsync(trainerProfile.Id, cancellationToken);
            return trainerCourses.Select(ToListItemDto).ToList();
        }

        throw new ConflictException("You must have a mentor profile or a trainer academy profile before managing courses.");
    }

    public async Task<CourseLessonDto> AddLessonAsync(Guid userId, Guid courseId, CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);
        await EnsureModuleBelongsToCourseAsync(request.ModuleId, courseId, cancellationToken);

        var now = DateTime.UtcNow;
        var lesson = new CourseLesson
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            ModuleId = request.ModuleId,
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
        await EnsureModuleBelongsToCourseAsync(request.ModuleId, courseId, cancellationToken);

        var lesson = await _courseRepository.GetLessonByIdAsync(lessonId, cancellationToken)
            ?? throw new NotFoundException("Lesson not found.");

        if (lesson.CourseId != courseId)
        {
            throw new NotFoundException("Lesson not found.");
        }

        lesson.ModuleId = request.ModuleId;
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

    public async Task<CourseModuleDto> AddModuleAsync(Guid userId, Guid courseId, CreateCourseModuleRequest request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var module = new CourseModule
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            DisplayOrder = request.DisplayOrder,
        };

        await _courseRepository.AddModuleAsync(module, cancellationToken);
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.SaveChangesAsync(cancellationToken);

        return ToModuleDto(module);
    }

    public async Task<CourseModuleDto> UpdateModuleAsync(
        Guid userId, Guid courseId, Guid moduleId, UpdateCourseModuleRequest request, CancellationToken cancellationToken)
    {
        await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var module = await _courseRepository.GetModuleByIdAsync(moduleId, cancellationToken)
            ?? throw new NotFoundException("Module not found.");

        if (module.CourseId != courseId)
        {
            throw new NotFoundException("Module not found.");
        }

        module.Title = request.Title.Trim();
        module.Description = request.Description.Trim();
        module.DisplayOrder = request.DisplayOrder;

        await _courseRepository.SaveChangesAsync(cancellationToken);
        return ToModuleDto(module);
    }

    public async Task DeleteModuleAsync(Guid userId, Guid courseId, Guid moduleId, CancellationToken cancellationToken)
    {
        await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var module = await _courseRepository.GetModuleByIdAsync(moduleId, cancellationToken)
            ?? throw new NotFoundException("Module not found.");

        if (module.CourseId != courseId)
        {
            throw new NotFoundException("Module not found.");
        }

        _courseRepository.RemoveModule(module);
        await _courseRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<CourseMaterialDto> AddMaterialAsync(Guid userId, Guid courseId, CreateCourseMaterialRequest request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        if (request.LessonId.HasValue && course.Lessons.All(l => l.Id != request.LessonId.Value))
        {
            throw new NotFoundException("Lesson not found in this course.");
        }

        var material = new CourseMaterial
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            LessonId = request.LessonId,
            Title = request.Title.Trim(),
            FileUrl = request.FileUrl.Trim(),
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
        };

        await _courseRepository.AddMaterialAsync(material, cancellationToken);
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.SaveChangesAsync(cancellationToken);

        return ToMaterialDto(material);
    }

    public async Task DeleteMaterialAsync(Guid userId, Guid courseId, Guid materialId, CancellationToken cancellationToken)
    {
        await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var material = await _courseRepository.GetMaterialByIdAsync(materialId, cancellationToken)
            ?? throw new NotFoundException("Material not found.");

        if (material.CourseId != courseId)
        {
            throw new NotFoundException("Material not found.");
        }

        _courseRepository.RemoveMaterial(material);
        await _courseRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<(MentorProfile? Mentor, AcademyMemberProfile? Trainer)> ResolveAuthorAsync(Guid userId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken);
        if (mentor is not null)
        {
            return (mentor, null);
        }

        var trainerProfile = await _academyMemberProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (trainerProfile is not null && trainerProfile.Role == AcademyMemberRole.Trainer)
        {
            return (null, trainerProfile);
        }

        throw new ConflictException("You must have a mentor profile or a trainer academy profile before managing courses.");
    }

    private async Task EnsureCategoryExistsAsync(Guid? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId.HasValue && await _categoryRepository.GetByIdAsync(categoryId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Course category not found.");
        }
    }

    private async Task EnsureModuleBelongsToCourseAsync(Guid? moduleId, Guid courseId, CancellationToken cancellationToken)
    {
        if (!moduleId.HasValue)
        {
            return;
        }

        var module = await _courseRepository.GetModuleByIdAsync(moduleId.Value, cancellationToken)
            ?? throw new NotFoundException("Module not found.");

        if (module.CourseId != courseId)
        {
            throw new NotFoundException("Module not found in this course.");
        }
    }

    private async Task<Course> GetOwnedCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        var isOwner = course.Mentor?.UserId == userId || course.TrainerProfile?.UserId == userId;
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this course.");
        }

        return course;
    }

    private static string AuthorNameOf(Course course)
        => course.Mentor?.User.FullName ?? course.TrainerProfile?.User.FullName ?? string.Empty;

    private static CourseListItemDto ToListItemDto(Course course) => new()
    {
        Id = course.Id,
        AuthorName = AuthorNameOf(course),
        Title = course.Title,
        Category = course.Category,
        CategoryName = course.CourseCategory?.Name,
        Status = course.Status.ToString(),
        LessonCount = course.Lessons.Count,
        MaxApprentices = course.MaxApprentices,
        ActiveEnrollmentCount = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
    };

    private static CourseDto ToDto(Course course) => new()
    {
        Id = course.Id,
        MentorId = course.MentorId,
        TrainerProfileId = course.TrainerProfileId,
        AuthorName = AuthorNameOf(course),
        Title = course.Title,
        Description = course.Description,
        Category = course.Category,
        CategoryId = course.CategoryId,
        CategoryName = course.CourseCategory?.Name,
        Status = course.Status.ToString(),
        MaxApprentices = course.MaxApprentices,
        ActiveEnrollmentCount = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
        Modules = course.Modules.OrderBy(m => m.DisplayOrder).Select(ToModuleDto).ToList(),
        Lessons = course.Lessons.OrderBy(l => l.DisplayOrder).Select(ToLessonDto).ToList(),
        Materials = course.Materials.OrderBy(m => m.DisplayOrder).Select(ToMaterialDto).ToList(),
        CreatedAt = course.CreatedAt,
        UpdatedAt = course.UpdatedAt,
        PublishedAt = course.PublishedAt,
    };

    private static CourseLessonDto ToLessonDto(CourseLesson lesson) => new()
    {
        Id = lesson.Id,
        CourseId = lesson.CourseId,
        ModuleId = lesson.ModuleId,
        Title = lesson.Title,
        Content = lesson.Content,
        VideoUrl = lesson.VideoUrl,
        DisplayOrder = lesson.DisplayOrder,
    };

    private static CourseModuleDto ToModuleDto(CourseModule module) => new()
    {
        Id = module.Id,
        CourseId = module.CourseId,
        Title = module.Title,
        Description = module.Description,
        DisplayOrder = module.DisplayOrder,
    };

    private static CourseMaterialDto ToMaterialDto(CourseMaterial material) => new()
    {
        Id = material.Id,
        CourseId = material.CourseId,
        LessonId = material.LessonId,
        Title = material.Title,
        FileUrl = material.FileUrl,
        DisplayOrder = material.DisplayOrder,
        CreatedAt = material.CreatedAt,
    };
}

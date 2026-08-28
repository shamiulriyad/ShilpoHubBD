using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICourseService
{
    Task<CourseDto> CreateAsync(Guid userId, CreateCourseRequest request, CancellationToken cancellationToken);
    Task<CourseDto> UpdateAsync(Guid userId, Guid courseId, UpdateCourseRequest request, CancellationToken cancellationToken);
    Task<CourseDto> PublishAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);
    Task<CourseDto> ArchiveAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);
    Task<CourseDto> GetByIdAsync(Guid courseId, Guid? currentUserId, CancellationToken cancellationToken);
    Task<PagedResult<CourseListItemDto>> GetPublishedAsync(CourseQueryParameters query, CancellationToken cancellationToken);
    Task<List<CourseListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken);
    Task<CourseLessonDto> AddLessonAsync(Guid userId, Guid courseId, CreateLessonRequest request, CancellationToken cancellationToken);
    Task<CourseLessonDto> UpdateLessonAsync(Guid userId, Guid courseId, Guid lessonId, UpdateLessonRequest request, CancellationToken cancellationToken);
    Task DeleteLessonAsync(Guid userId, Guid courseId, Guid lessonId, CancellationToken cancellationToken);

    Task<CourseModuleDto> AddModuleAsync(Guid userId, Guid courseId, CreateCourseModuleRequest request, CancellationToken cancellationToken);
    Task<CourseModuleDto> UpdateModuleAsync(Guid userId, Guid courseId, Guid moduleId, UpdateCourseModuleRequest request, CancellationToken cancellationToken);
    Task DeleteModuleAsync(Guid userId, Guid courseId, Guid moduleId, CancellationToken cancellationToken);

    Task<CourseMaterialDto> AddMaterialAsync(Guid userId, Guid courseId, CreateCourseMaterialRequest request, CancellationToken cancellationToken);
    Task DeleteMaterialAsync(Guid userId, Guid courseId, Guid materialId, CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICourseService
{
    Task<CourseDto> CreateAsync(Guid userId, CreateCourseRequest request, CancellationToken cancellationToken);
    Task<CourseDto> UpdateAsync(Guid userId, Guid courseId, UpdateCourseRequest request, CancellationToken cancellationToken);
    Task<CourseDto> PublishAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);
    Task<CourseDto> ArchiveAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);
    Task<CourseDto> GetByIdAsync(Guid courseId, Guid? currentUserId, CancellationToken cancellationToken);
    Task<PagedResult<CourseListItemDto>> GetPublishedAsync(CourseQueryParameters query, CancellationToken cancellationToken);
    Task<List<CourseListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken);
    Task<CourseLessonDto> AddLessonAsync(Guid userId, Guid courseId, CreateLessonRequest request, CancellationToken cancellationToken);
    Task<CourseLessonDto> UpdateLessonAsync(Guid userId, Guid courseId, Guid lessonId, UpdateLessonRequest request, CancellationToken cancellationToken);
    Task DeleteLessonAsync(Guid userId, Guid courseId, Guid lessonId, CancellationToken cancellationToken);
}

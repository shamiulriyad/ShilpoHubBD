using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CourseLesson?> GetLessonByIdAsync(Guid lessonId, CancellationToken cancellationToken);
    Task<(List<Course> Items, int TotalCount)> GetPagedAsync(CourseQueryParameters query, CancellationToken cancellationToken);
    Task<List<Course>> GetByMentorAsync(Guid mentorId, CancellationToken cancellationToken);
    Task AddAsync(Course course, CancellationToken cancellationToken);
    Task AddLessonAsync(CourseLesson lesson, CancellationToken cancellationToken);
    void RemoveLesson(CourseLesson lesson);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

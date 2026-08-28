using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CourseLesson?> GetLessonByIdAsync(Guid lessonId, CancellationToken cancellationToken);
    Task<(List<Course> Items, int TotalCount)> GetPagedAsync(CourseQueryParameters query, CancellationToken cancellationToken);
    Task<List<Course>> GetByMentorAsync(Guid mentorId, CancellationToken cancellationToken);
    Task<List<Course>> GetByTrainerProfileAsync(Guid trainerProfileId, CancellationToken cancellationToken);
    Task AddAsync(Course course, CancellationToken cancellationToken);
    void Remove(Course course);
    Task AddLessonAsync(CourseLesson lesson, CancellationToken cancellationToken);
    void RemoveLesson(CourseLesson lesson);

    Task<CourseModule?> GetModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken);
    Task AddModuleAsync(CourseModule module, CancellationToken cancellationToken);
    void RemoveModule(CourseModule module);

    Task<CourseMaterial?> GetMaterialByIdAsync(Guid materialId, CancellationToken cancellationToken);
    Task AddMaterialAsync(CourseMaterial material, CancellationToken cancellationToken);
    void RemoveMaterial(CourseMaterial material);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

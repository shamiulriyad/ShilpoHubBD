using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ShilpoHubDbContext _context;

    public CourseRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Course> WithDetails()
        => _context.Courses
            .Include(c => c.Mentor).ThenInclude(m => m!.User)
            .Include(c => c.TrainerProfile).ThenInclude(t => t!.User)
            .Include(c => c.CourseCategory)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Include(c => c.Modules)
            .Include(c => c.Materials)
            .AsSplitQuery();

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<CourseLesson?> GetLessonByIdAsync(Guid lessonId, CancellationToken cancellationToken)
        => _context.CourseLessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken);

    public async Task<(List<Course> Items, int TotalCount)> GetPagedAsync(CourseQueryParameters query, CancellationToken cancellationToken)
    {
        var courses = WithDetails().Where(c => c.Status == CourseStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            courses = courses.Where(c => c.Category == query.Category);
        }

        if (query.MentorId.HasValue)
        {
            courses = courses.Where(c => c.MentorId == query.MentorId.Value);
        }

        courses = courses.OrderByDescending(c => c.PublishedAt);

        var totalCount = await courses.CountAsync(cancellationToken);
        var items = await courses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<Course>> GetByMentorAsync(Guid mentorId, CancellationToken cancellationToken)
        => WithDetails().Where(c => c.MentorId == mentorId).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);

    public Task<List<Course>> GetByTrainerProfileAsync(Guid trainerProfileId, CancellationToken cancellationToken)
        => WithDetails().Where(c => c.TrainerProfileId == trainerProfileId).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Course course, CancellationToken cancellationToken)
        => await _context.Courses.AddAsync(course, cancellationToken);

    public void Remove(Course course)
        => _context.Courses.Remove(course);

    public async Task AddLessonAsync(CourseLesson lesson, CancellationToken cancellationToken)
        => await _context.CourseLessons.AddAsync(lesson, cancellationToken);

    public void RemoveLesson(CourseLesson lesson)
        => _context.CourseLessons.Remove(lesson);

    public Task<CourseModule?> GetModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken)
        => _context.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);

    public async Task AddModuleAsync(CourseModule module, CancellationToken cancellationToken)
        => await _context.CourseModules.AddAsync(module, cancellationToken);

    public void RemoveModule(CourseModule module)
        => _context.CourseModules.Remove(module);

    public Task<CourseMaterial?> GetMaterialByIdAsync(Guid materialId, CancellationToken cancellationToken)
        => _context.CourseMaterials.FirstOrDefaultAsync(m => m.Id == materialId, cancellationToken);

    public async Task AddMaterialAsync(CourseMaterial material, CancellationToken cancellationToken)
        => await _context.CourseMaterials.AddAsync(material, cancellationToken);

    public void RemoveMaterial(CourseMaterial material)
        => _context.CourseMaterials.Remove(material);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.Roadmap;

namespace ShilpoHubBD.Data.Repositories;

public class LearningRoadmapRepository : ILearningRoadmapRepository
{
    private readonly ShilpoHubDbContext _context;

    public LearningRoadmapRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<LearningRoadmap> WithDetails()
        => _context.LearningRoadmaps
            .Include(r => r.AcademyMemberProfile)
            .Include(r => r.TargetHeritageSkill)
            .Include(r => r.Milestones).ThenInclude(m => m.HeritageSkill)
            .Include(r => r.Milestones).ThenInclude(m => m.RecommendedCourses).ThenInclude(c => c.Course)
            .Include(r => r.Milestones).ThenInclude(m => m.RecommendedLessons).ThenInclude(l => l.CourseLesson)
            .AsSplitQuery();

    public Task<LearningRoadmap?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<LearningRoadmap?> GetActiveByProfileAsync(Guid academyMemberProfileId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(
            r => r.AcademyMemberProfileId == academyMemberProfileId && r.Status == RoadmapStatus.Active, cancellationToken);

    public Task<List<LearningRoadmap>> GetByProfileAsync(Guid academyMemberProfileId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(r => r.AcademyMemberProfileId == academyMemberProfileId)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(LearningRoadmap roadmap, CancellationToken cancellationToken)
        => await _context.LearningRoadmaps.AddAsync(roadmap, cancellationToken);

    public Task<RoadmapMilestone?> GetMilestoneByIdAsync(Guid milestoneId, CancellationToken cancellationToken)
        => _context.RoadmapMilestones
            .Include(m => m.LearningRoadmap)
            .FirstOrDefaultAsync(m => m.Id == milestoneId, cancellationToken);

    public Task<List<Course>> FindCandidateCoursesAsync(
        string skillName, int courseTake, int lessonTakePerCourse, CancellationToken cancellationToken)
        => _context.Courses
            .Where(c => c.Status == CourseStatus.Published
                && (EF.Functions.ILike(c.Title, $"%{skillName}%") || EF.Functions.ILike(c.Category, $"%{skillName}%")))
            .Include(c => c.Lessons.OrderBy(l => l.DisplayOrder).Take(lessonTakePerCourse))
            .OrderByDescending(c => c.PublishedAt)
            .Take(courseTake)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

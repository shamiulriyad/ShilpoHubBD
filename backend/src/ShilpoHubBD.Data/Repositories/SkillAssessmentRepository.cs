using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.SkillAssessment;

namespace ShilpoHubBD.Data.Repositories;

public class SkillAssessmentRepository : ISkillAssessmentRepository
{
    private readonly ShilpoHubDbContext _context;

    public SkillAssessmentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<SkillAssessment> WithDetails()
        => _context.SkillAssessments
            .Include(a => a.AcademyMemberProfile)
            .Include(a => a.HeritageSkill)
            .Include(a => a.Insights)
            .Include(a => a.RecommendedSkills).ThenInclude(r => r.HeritageSkill)
            .AsSplitQuery();

    public Task<SkillAssessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<List<SkillAssessment>> GetByProfileAsync(Guid academyMemberProfileId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(a => a.AcademyMemberProfileId == academyMemberProfileId)
            .OrderByDescending(a => a.AssessedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(SkillAssessment assessment, CancellationToken cancellationToken)
        => await _context.SkillAssessments.AddAsync(assessment, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

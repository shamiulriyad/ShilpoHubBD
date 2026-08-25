using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Repositories;

public class MentorRepository : IMentorRepository
{
    private readonly ShilpoHubDbContext _context;

    public MentorRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<MentorProfile> WithDetails()
        => _context.MentorProfiles
            .Include(m => m.User)
            .Include(m => m.Courses)
            .Include(m => m.Skills).ThenInclude(s => s.HeritageSkill)
            .AsSplitQuery();

    public Task<MentorProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<MentorProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

    public async Task<(List<MentorProfile> Items, int TotalCount)> GetPagedAsync(
        bool activeOnly, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = WithDetails().AsQueryable();

        if (activeOnly)
        {
            query = query.Where(m => m.IsActive);
        }

        query = query.OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(MentorProfile mentor, CancellationToken cancellationToken)
        => await _context.MentorProfiles.AddAsync(mentor, cancellationToken);

    public Task<MentorSkill?> GetSkillAsync(Guid mentorProfileId, Guid heritageSkillId, CancellationToken cancellationToken)
        => _context.MentorSkills.FirstOrDefaultAsync(
            s => s.MentorProfileId == mentorProfileId && s.HeritageSkillId == heritageSkillId, cancellationToken);

    public async Task AddSkillAsync(MentorSkill skill, CancellationToken cancellationToken)
        => await _context.MentorSkills.AddAsync(skill, cancellationToken);

    public void RemoveSkill(MentorSkill skill)
        => _context.MentorSkills.Remove(skill);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

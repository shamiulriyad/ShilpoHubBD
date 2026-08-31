using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Repositories;

public class AcademyMemberProfileRepository : IAcademyMemberProfileRepository
{
    private readonly ShilpoHubDbContext _context;

    public AcademyMemberProfileRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<AcademyMemberProfile> WithDetails()
        => _context.AcademyMemberProfiles
            .Include(p => p.User)
            .Include(p => p.Skills).ThenInclude(s => s.HeritageSkill)
            .AsSplitQuery();

    public Task<AcademyMemberProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<AcademyMemberProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task AddAsync(AcademyMemberProfile profile, CancellationToken cancellationToken)
        => await _context.AcademyMemberProfiles.AddAsync(profile, cancellationToken);

    public Task<AcademyMemberSkill?> GetSkillAsync(Guid profileId, Guid heritageSkillId, CancellationToken cancellationToken)
        => _context.AcademyMemberSkills.FirstOrDefaultAsync(
            s => s.AcademyMemberProfileId == profileId && s.HeritageSkillId == heritageSkillId, cancellationToken);

    public async Task AddSkillAsync(AcademyMemberSkill skill, CancellationToken cancellationToken)
        => await _context.AcademyMemberSkills.AddAsync(skill, cancellationToken);

    public void RemoveSkill(AcademyMemberSkill skill)
        => _context.AcademyMemberSkills.Remove(skill);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

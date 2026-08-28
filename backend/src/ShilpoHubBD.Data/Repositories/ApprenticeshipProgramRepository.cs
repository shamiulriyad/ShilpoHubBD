using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Data.Repositories;

public class ApprenticeshipProgramRepository : IApprenticeshipProgramRepository
{
    private readonly ShilpoHubDbContext _context;

    public ApprenticeshipProgramRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ApprenticeshipProgram> WithDetails()
        => _context.ApprenticeshipPrograms
            .Include(p => p.Mentor).ThenInclude(m => m!.User)
            .Include(p => p.TrainerProfile).ThenInclude(t => t!.User)
            .Include(p => p.HeritageSkill)
            .Include(p => p.Milestones)
            .Include(p => p.Enrollments)
            .AsSplitQuery();

    public Task<ApprenticeshipProgram?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(List<ApprenticeshipProgram> Items, int TotalCount)> GetPagedAsync(
        ApprenticeshipProgramQueryParameters query, CancellationToken cancellationToken)
    {
        var programs = WithDetails().Where(p => p.Status == ProgramStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Type) && Enum.TryParse<ProgramType>(query.Type, true, out var type))
        {
            programs = programs.Where(p => p.Type == type);
        }

        if (query.HeritageSkillId.HasValue)
        {
            programs = programs.Where(p => p.HeritageSkillId == query.HeritageSkillId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            programs = programs.Where(p => p.Location == query.Location);
        }

        programs = programs.OrderByDescending(p => p.PublishedAt);

        var totalCount = await programs.CountAsync(cancellationToken);
        var items = await programs
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<ApprenticeshipProgram>> GetByMentorAsync(Guid mentorId, CancellationToken cancellationToken)
        => WithDetails().Where(p => p.MentorId == mentorId).OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);

    public Task<List<ApprenticeshipProgram>> GetByTrainerProfileAsync(Guid trainerProfileId, CancellationToken cancellationToken)
        => WithDetails().Where(p => p.TrainerProfileId == trainerProfileId).OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(ApprenticeshipProgram program, CancellationToken cancellationToken)
        => await _context.ApprenticeshipPrograms.AddAsync(program, cancellationToken);

    public void Remove(ApprenticeshipProgram program)
        => _context.ApprenticeshipPrograms.Remove(program);

    public Task<TrainingMilestone?> GetMilestoneByIdAsync(Guid milestoneId, CancellationToken cancellationToken)
        => _context.TrainingMilestones.FirstOrDefaultAsync(m => m.Id == milestoneId, cancellationToken);

    public async Task AddMilestoneAsync(TrainingMilestone milestone, CancellationToken cancellationToken)
        => await _context.TrainingMilestones.AddAsync(milestone, cancellationToken);

    public void RemoveMilestone(TrainingMilestone milestone)
        => _context.TrainingMilestones.Remove(milestone);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

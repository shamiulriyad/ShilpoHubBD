using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Data.Repositories;

public class ApprenticeEnrollmentRepository : IApprenticeEnrollmentRepository
{
    private readonly ShilpoHubDbContext _context;

    public ApprenticeEnrollmentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ApprenticeEnrollment> WithDetails()
        => _context.ApprenticeEnrollments
            .Include(e => e.Program).ThenInclude(p => p.Milestones)
            .Include(e => e.Program).ThenInclude(p => p.Mentor).ThenInclude(m => m!.User)
            .Include(e => e.Program).ThenInclude(p => p.TrainerProfile).ThenInclude(t => t!.User)
            .Include(e => e.Apprentice)
            .Include(e => e.MilestoneProgress).ThenInclude(p => p.Milestone)
            .AsSplitQuery();

    public Task<ApprenticeEnrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<ApprenticeEnrollment?> GetByProgramAndApprenticeAsync(Guid programId, Guid apprenticeUserId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(e => e.ProgramId == programId && e.ApprenticeUserId == apprenticeUserId, cancellationToken);

    public Task<List<ApprenticeEnrollment>> GetByApprenticeAsync(Guid apprenticeUserId, CancellationToken cancellationToken)
        => WithDetails().Where(e => e.ApprenticeUserId == apprenticeUserId).OrderByDescending(e => e.EnrolledAt).ToListAsync(cancellationToken);

    public Task<List<ApprenticeEnrollment>> GetByProgramAsync(Guid programId, CancellationToken cancellationToken)
        => WithDetails().Where(e => e.ProgramId == programId).OrderByDescending(e => e.EnrolledAt).ToListAsync(cancellationToken);

    public Task<int> GetActiveCountByProgramAsync(Guid programId, CancellationToken cancellationToken)
        => _context.ApprenticeEnrollments.CountAsync(
            e => e.ProgramId == programId && e.Status == ApprenticeEnrollmentStatus.Active, cancellationToken);

    public async Task AddAsync(ApprenticeEnrollment enrollment, CancellationToken cancellationToken)
        => await _context.ApprenticeEnrollments.AddAsync(enrollment, cancellationToken);

    public async Task AddMilestoneProgressAsync(ApprenticeMilestoneProgress progress, CancellationToken cancellationToken)
        => await _context.ApprenticeMilestoneProgress.AddAsync(progress, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

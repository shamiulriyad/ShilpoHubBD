using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Data.Repositories;

public class ProgramApplicationRepository : IProgramApplicationRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProgramApplicationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProgramApplication> WithDetails()
        => _context.ProgramApplications
            .Include(a => a.Program).ThenInclude(p => p.Mentor).ThenInclude(m => m!.User)
            .Include(a => a.Program).ThenInclude(p => p.TrainerProfile).ThenInclude(t => t!.User)
            .Include(a => a.Applicant)
            .AsSplitQuery();

    public Task<ProgramApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<List<ProgramApplication>> GetByApplicantAsync(Guid applicantUserId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(a => a.ApplicantUserId == applicantUserId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(cancellationToken);

    public Task<List<ProgramApplication>> GetByProgramAsync(Guid programId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(a => a.ProgramId == programId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasOpenApplicationAsync(Guid programId, Guid applicantUserId, CancellationToken cancellationToken)
        => _context.ProgramApplications.AnyAsync(
            a => a.ProgramId == programId
                && a.ApplicantUserId == applicantUserId
                && (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Accepted),
            cancellationToken);

    public async Task AddAsync(ProgramApplication application, CancellationToken cancellationToken)
        => await _context.ProgramApplications.AddAsync(application, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

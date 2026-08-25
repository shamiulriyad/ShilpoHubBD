using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Data.Repositories;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ShilpoHubDbContext _context;

    public JobApplicationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<JobApplication> WithDetails()
        => _context.JobApplications
            .Include(a => a.JobListing).ThenInclude(j => j.BusinessPartnerProfile)
            .Include(a => a.Applicant)
            .AsSplitQuery();

    public Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<List<JobApplication>> GetByApplicantAsync(Guid applicantUserId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(a => a.ApplicantUserId == applicantUserId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(cancellationToken);

    public Task<List<JobApplication>> GetByJobListingAsync(Guid jobListingId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(a => a.JobListingId == jobListingId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasOpenApplicationAsync(Guid jobListingId, Guid applicantUserId, CancellationToken cancellationToken)
        => _context.JobApplications.AnyAsync(
            a => a.JobListingId == jobListingId
                && a.ApplicantUserId == applicantUserId
                && (a.Status == JobApplicationStatus.Pending || a.Status == JobApplicationStatus.Shortlisted),
            cancellationToken);

    public async Task AddAsync(JobApplication application, CancellationToken cancellationToken)
        => await _context.JobApplications.AddAsync(application, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

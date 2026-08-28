using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Data.Repositories;

public class JobListingRepository : IJobListingRepository
{
    private readonly ShilpoHubDbContext _context;

    public JobListingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<JobListing> WithDetails()
        => _context.JobListings
            .Include(j => j.BusinessPartnerProfile)
            .Include(j => j.SkillRequirements).ThenInclude(r => r.HeritageSkill)
            .Include(j => j.Applications)
            .AsSplitQuery();

    public Task<JobListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<(List<JobListing> Items, int TotalCount)> GetPagedAsync(JobListingQueryParameters query, CancellationToken cancellationToken)
    {
        var jobs = WithDetails().Where(j => j.Status == JobListingStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.EmploymentType) && Enum.TryParse<EmploymentType>(query.EmploymentType, true, out var employmentType))
        {
            jobs = jobs.Where(j => j.EmploymentType == employmentType);
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            jobs = jobs.Where(j => j.Location == query.Location);
        }

        if (query.HeritageSkillId.HasValue)
        {
            jobs = jobs.Where(j => j.SkillRequirements.Any(r => r.HeritageSkillId == query.HeritageSkillId.Value));
        }

        jobs = jobs.OrderByDescending(j => j.PublishedAt);

        var totalCount = await jobs.CountAsync(cancellationToken);
        var items = await jobs
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<JobListing>> GetByEmployerAsync(Guid businessPartnerProfileId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(j => j.BusinessPartnerProfileId == businessPartnerProfileId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<List<JobListing>> GetPublishedForMatchingAsync(CancellationToken cancellationToken)
        => WithDetails()
            .Where(j => j.Status == JobListingStatus.Published)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(JobListing jobListing, CancellationToken cancellationToken)
        => await _context.JobListings.AddAsync(jobListing, cancellationToken);

    public void Remove(JobListing jobListing)
        => _context.JobListings.Remove(jobListing);

    public Task<JobSkillRequirement?> GetSkillRequirementByIdAsync(Guid requirementId, CancellationToken cancellationToken)
        => _context.JobSkillRequirements.FirstOrDefaultAsync(r => r.Id == requirementId, cancellationToken);

    public async Task AddSkillRequirementAsync(JobSkillRequirement requirement, CancellationToken cancellationToken)
        => await _context.JobSkillRequirements.AddAsync(requirement, cancellationToken);

    public void RemoveSkillRequirement(JobSkillRequirement requirement)
        => _context.JobSkillRequirements.Remove(requirement);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

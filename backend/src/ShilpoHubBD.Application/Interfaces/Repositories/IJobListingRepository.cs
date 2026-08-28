using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IJobListingRepository
{
    Task<JobListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<JobListing> Items, int TotalCount)> GetPagedAsync(JobListingQueryParameters query, CancellationToken cancellationToken);
    Task<List<JobListing>> GetByEmployerAsync(Guid businessPartnerProfileId, CancellationToken cancellationToken);
    Task<List<JobListing>> GetPublishedForMatchingAsync(CancellationToken cancellationToken);
    Task AddAsync(JobListing jobListing, CancellationToken cancellationToken);
    void Remove(JobListing jobListing);

    Task<JobSkillRequirement?> GetSkillRequirementByIdAsync(Guid requirementId, CancellationToken cancellationToken);
    Task AddSkillRequirementAsync(JobSkillRequirement requirement, CancellationToken cancellationToken);
    void RemoveSkillRequirement(JobSkillRequirement requirement);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Employment;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IJobListingService
{
    Task<JobListingDto> CreateAsync(Guid userId, CreateJobListingRequest request, CancellationToken cancellationToken);

    Task<JobListingDto> UpdateAsync(Guid userId, Guid jobListingId, UpdateJobListingRequest request, CancellationToken cancellationToken);

    Task<JobListingDto> PublishAsync(Guid userId, Guid jobListingId, CancellationToken cancellationToken);

    Task<JobListingDto> CloseAsync(Guid userId, Guid jobListingId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid jobListingId, CancellationToken cancellationToken);

    Task<JobListingDto> GetByIdAsync(Guid jobListingId, Guid? currentUserId, CancellationToken cancellationToken);

    Task<PagedResult<JobListingListItemDto>> GetPublishedAsync(JobListingQueryParameters query, CancellationToken cancellationToken);

    Task<List<JobListingListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken);

    Task<JobSkillRequirementDto> AddSkillRequirementAsync(Guid userId, Guid jobListingId, AddJobSkillRequirementRequest request, CancellationToken cancellationToken);

    Task RemoveSkillRequirementAsync(Guid userId, Guid jobListingId, Guid requirementId, CancellationToken cancellationToken);
}

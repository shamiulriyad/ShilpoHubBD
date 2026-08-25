using ShilpoHubBD.Application.DTOs.Employment;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IJobMatchingService
{
    Task<List<JobMatchResultDto>> GetRecommendedJobsAsync(Guid userId, JobMatchRequest request, CancellationToken cancellationToken);
}

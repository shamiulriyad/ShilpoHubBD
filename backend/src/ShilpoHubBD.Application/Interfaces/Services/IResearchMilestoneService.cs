using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IResearchMilestoneService
{
    Task<List<ResearchMilestoneDto>> GetForProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);

    Task<ResearchMilestoneDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchMilestoneRequest request, CancellationToken cancellationToken);

    Task<ResearchMilestoneDto> UpdateAsync(
        Guid userId, Guid projectId, Guid milestoneId, UpdateResearchMilestoneRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid projectId, Guid milestoneId, CancellationToken cancellationToken);
}

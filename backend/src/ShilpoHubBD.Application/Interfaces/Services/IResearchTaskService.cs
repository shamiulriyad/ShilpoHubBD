using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IResearchTaskService
{
    Task<PagedResult<ResearchTaskDto>> GetForProjectAsync(
        Guid userId, Guid projectId, ResearchTaskQueryParameters query, CancellationToken cancellationToken);

    Task<ResearchTaskDto> GetByIdAsync(Guid userId, Guid projectId, Guid taskId, CancellationToken cancellationToken);

    Task<ResearchTaskDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchTaskRequest request, CancellationToken cancellationToken);

    Task<ResearchTaskDto> UpdateAsync(
        Guid userId, Guid projectId, Guid taskId, UpdateResearchTaskRequest request, CancellationToken cancellationToken);

    Task<ResearchTaskDto> UpdateStatusAsync(
        Guid userId, Guid projectId, Guid taskId, UpdateResearchTaskStatusRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid projectId, Guid taskId, CancellationToken cancellationToken);
}

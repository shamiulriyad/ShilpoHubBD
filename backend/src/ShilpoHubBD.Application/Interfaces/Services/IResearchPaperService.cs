using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IResearchPaperService
{
    Task<List<ResearchPaperDto>> GetForProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);

    Task<ResearchPaperDto> GetByIdAsync(Guid userId, Guid projectId, Guid paperId, CancellationToken cancellationToken);

    Task<ResearchPaperDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchPaperRequest request, CancellationToken cancellationToken);

    Task<ResearchPaperDto> UpdateAsync(
        Guid userId, Guid projectId, Guid paperId, UpdateResearchPaperRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid projectId, Guid paperId, CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IResearchAIAnalysisRepository
{
    Task<ResearchAIAnalysis?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<ResearchAIAnalysis> Items, int TotalCount)> GetPagedForProjectAsync(
        Guid projectId, ResearchAIAnalysisQueryParameters query, CancellationToken cancellationToken);

    Task AddAsync(ResearchAIAnalysis analysis, CancellationToken cancellationToken);
    void Remove(ResearchAIAnalysis analysis);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

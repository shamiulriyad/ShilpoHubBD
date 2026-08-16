using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICulturalStoryRepository
{
    Task<(List<CulturalStory> Items, int TotalCount)> GetPagedAsync(CulturalStoryQueryParameters query, CancellationToken cancellationToken);
    Task<CulturalStory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(CulturalStory story, CancellationToken cancellationToken);
    void Remove(CulturalStory story);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

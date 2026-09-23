using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ISavedTourPlanRepository
{
    Task<(List<SavedTourPlan> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<SavedTourPlan?> GetForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task AddAsync(SavedTourPlan plan, CancellationToken cancellationToken);
    void Remove(SavedTourPlan plan);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

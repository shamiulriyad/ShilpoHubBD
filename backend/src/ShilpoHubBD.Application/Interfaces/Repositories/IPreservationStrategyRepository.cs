using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPreservationStrategyRepository
{
    Task<PreservationStrategy?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PreservationStrategy?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<PreservationStrategy> Items, int TotalCount)> GetPagedForOwnerAsync(
        Guid ownerUserId, PreservationStrategyQueryParameters query, CancellationToken cancellationToken);
    Task AddAsync(PreservationStrategy strategy, CancellationToken cancellationToken);
    void Remove(PreservationStrategy strategy);

    Task<StrategyObjective?> GetObjectiveByIdAsync(Guid objectiveId, CancellationToken cancellationToken);
    Task<List<StrategyObjective>> GetObjectivesAsync(Guid strategyId, CancellationToken cancellationToken);
    Task AddObjectiveAsync(StrategyObjective objective, CancellationToken cancellationToken);
    void RemoveObjective(StrategyObjective objective);

    Task<StrategyAction?> GetActionByIdAsync(Guid actionId, CancellationToken cancellationToken);
    Task<List<StrategyAction>> GetActionsAsync(Guid strategyId, CancellationToken cancellationToken);
    Task AddActionAsync(StrategyAction action, CancellationToken cancellationToken);
    void RemoveAction(StrategyAction action);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

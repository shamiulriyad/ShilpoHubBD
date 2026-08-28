using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPreservationStrategyService
{
    Task<PagedResult<PreservationStrategyListItemDto>> GetMineAsync(
        Guid userId, PreservationStrategyQueryParameters query, CancellationToken cancellationToken);

    Task<PreservationStrategyDetailDto> GetByIdAsync(Guid userId, Guid strategyId, CancellationToken cancellationToken);

    Task<PreservationStrategyDetailDto> CreateAsync(
        Guid userId, bool isResearcher, CreatePreservationStrategyRequest request, CancellationToken cancellationToken);

    Task<PreservationStrategyDetailDto> UpdateAsync(
        Guid userId, bool isResearcher, Guid strategyId, UpdatePreservationStrategyRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid strategyId, CancellationToken cancellationToken);

    Task<StrategyObjectiveDto> AddObjectiveAsync(
        Guid userId, Guid strategyId, CreateStrategyObjectiveRequest request, CancellationToken cancellationToken);

    Task<StrategyObjectiveDto> UpdateObjectiveAsync(
        Guid userId, Guid strategyId, Guid objectiveId, UpdateStrategyObjectiveRequest request, CancellationToken cancellationToken);

    Task DeleteObjectiveAsync(Guid userId, Guid strategyId, Guid objectiveId, CancellationToken cancellationToken);

    Task<StrategyActionDto> AddActionAsync(
        Guid userId, Guid strategyId, CreateStrategyActionRequest request, CancellationToken cancellationToken);

    Task<StrategyActionDto> UpdateActionAsync(
        Guid userId, Guid strategyId, Guid actionId, UpdateStrategyActionRequest request, CancellationToken cancellationToken);

    Task DeleteActionAsync(Guid userId, Guid strategyId, Guid actionId, CancellationToken cancellationToken);
}

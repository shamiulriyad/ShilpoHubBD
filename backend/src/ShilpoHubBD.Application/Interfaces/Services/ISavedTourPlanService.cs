using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISavedTourPlanService
{
    Task<Guid> SaveAsync(Guid userId, TourPlanRequest request, TourPlanResult result, CancellationToken cancellationToken);
    Task<PagedResult<SavedTourPlanSummaryDto>> GetMyPlansAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<SavedTourPlanDto> GetMyPlanAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task DeleteMyPlanAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}

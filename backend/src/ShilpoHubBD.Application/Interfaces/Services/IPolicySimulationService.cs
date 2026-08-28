using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPolicySimulationService
{
    Task<PolicySimulationDto> RunAsync(
        Guid userId, RunPolicySimulationRequest request, CancellationToken cancellationToken);

    Task<PagedResult<PolicySimulationListItemDto>> GetSimulationsAsync(
        PolicySimulationQueryParameters query, CancellationToken cancellationToken);

    Task<PolicySimulationDto> GetSimulationByIdAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteSimulationAsync(Guid id, CancellationToken cancellationToken);
}

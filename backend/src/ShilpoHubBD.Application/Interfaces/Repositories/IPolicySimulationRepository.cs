using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPolicySimulationRepository
{
    Task<PolicyBaselineSignals> GatherBaselineAsync(
        HeritageIndexScope scope, Guid? scopeId, DateTime from, DateTime to, CancellationToken cancellationToken);

    Task<GovScopeRef?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken);

    Task<GovVillageRef?> GetVillageAsync(Guid villageId, CancellationToken cancellationToken);

    Task AddAsync(PolicySimulation simulation, CancellationToken cancellationToken);

    void Remove(PolicySimulation simulation);

    Task<PolicySimulation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<PolicySimulation> Items, int TotalCount)> GetPagedAsync(
        PolicySimulationQueryParameters query, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

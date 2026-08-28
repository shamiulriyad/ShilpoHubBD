using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

/// <summary>
/// Gathers scope-filtered live signals for the Heritage Intelligence indices and persists the
/// computed <see cref="HeritageIndexRecord"/>s.
/// </summary>
public interface IHeritageIntelligenceRepository
{
    Task<HeritageIntelligenceSignals> GatherSignalsAsync(
        HeritageIndexScope scope,
        Guid? scopeId,
        string? craftLabel,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    Task<GovScopeRef?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken);

    Task<GovVillageRef?> GetVillageAsync(Guid villageId, CancellationToken cancellationToken);

    Task AddAsync(HeritageIndexRecord record, CancellationToken cancellationToken);

    void Remove(HeritageIndexRecord record);

    Task<HeritageIndexRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<HeritageIndexRecord> Items, int TotalCount)> GetPagedAsync(
        HeritageIndexQueryParameters query, CancellationToken cancellationToken);

    Task<List<HeritageIndexRecord>> GetForTrendAsync(
        HeritageIndexType indexType,
        HeritageIndexScope scope,
        Guid? scopeId,
        string? craftLabel,
        int take,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public record GovScopeRef(Guid Id, string Name, string Division);

public record GovVillageRef(Guid Id, string Name, string Craft, Guid DistrictId, string DistrictName);

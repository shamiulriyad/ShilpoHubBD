using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageIntelligenceService
{
    Task<HeritageIndexRecordDto> ComputeAsync(
        Guid userId, ComputeHeritageIndexRequest request, CancellationToken cancellationToken);

    Task<PagedResult<HeritageIndexRecordListItemDto>> GetRecordsAsync(
        HeritageIndexQueryParameters query, CancellationToken cancellationToken);

    Task<HeritageIndexRecordDto> GetRecordByIdAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteRecordAsync(Guid id, CancellationToken cancellationToken);

    Task<HeritageIndexTrendDto> GetTrendAsync(
        string indexType, string scope, Guid? scopeId, string? craftLabel, int take,
        CancellationToken cancellationToken);
}

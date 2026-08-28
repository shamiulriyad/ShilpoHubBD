using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageRiskService
{
    Task<PagedResult<HeritageRiskRecordDto>> GetPagedAsync(HeritageRiskQueryParameters query, CancellationToken cancellationToken);
    Task<HeritageRiskRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HeritageRiskRecordDto> CreateAsync(Guid userId, CreateHeritageRiskRecordRequest request, CancellationToken cancellationToken);
    Task<HeritageRiskRecordDto> UpdateAsync(Guid userId, Guid id, UpdateHeritageRiskRecordRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

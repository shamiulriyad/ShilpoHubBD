using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IUnescoRecordService
{
    Task<List<UnescoRecordDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<UnescoRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UnescoRecordDto> CreateAsync(CreateUnescoRecordRequest request, CancellationToken cancellationToken);

    Task<UnescoRecordDto> UpdateAsync(Guid id, UpdateUnescoRecordRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

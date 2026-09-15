using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IUnescoRecordRepository
{
    Task<List<UnescoRecord>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<UnescoRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken);

    Task AddAsync(UnescoRecord record, CancellationToken cancellationToken);

    void Remove(UnescoRecord record);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

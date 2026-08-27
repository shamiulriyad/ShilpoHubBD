using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritageRiskRepository
{
    Task<HeritageRiskRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<HeritageRiskRecord> Items, int TotalCount)> GetPagedAsync(
        HeritageRiskQueryParameters query, CancellationToken cancellationToken);
    Task AddAsync(HeritageRiskRecord record, CancellationToken cancellationToken);
    void Remove(HeritageRiskRecord record);

    Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken);
    Task<bool> VillageExistsAsync(Guid villageId, CancellationToken cancellationToken);
    Task<bool> ProducerExistsAsync(Guid producerId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ITouristServiceRepository
{
    Task<(List<TouristService> Items, int TotalCount)> GetPagedAsync(TouristServiceQueryParameters query, CancellationToken cancellationToken);

    Task<(List<TouristService> Items, int TotalCount)> GetPagedByProducerAsync(
        Guid producerId, int page, int pageSize, CancellationToken cancellationToken);

    Task<TouristService?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(TouristService service, CancellationToken cancellationToken);
    void Remove(TouristService service);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

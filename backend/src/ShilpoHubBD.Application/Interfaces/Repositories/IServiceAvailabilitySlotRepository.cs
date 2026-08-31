using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IServiceAvailabilitySlotRepository
{
    Task<(List<ServiceAvailabilitySlot> Items, int TotalCount)> GetPagedByServiceAsync(
        Guid serviceId, AvailabilitySlotQueryParameters query, CancellationToken cancellationToken);

    Task<ServiceAvailabilitySlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ServiceAvailabilitySlot slot, CancellationToken cancellationToken);
    void Remove(ServiceAvailabilitySlot slot);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

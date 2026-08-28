using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.TouristBooking;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IServiceAvailabilityService
{
    Task<PagedResult<ServiceAvailabilitySlotDto>> GetPagedByServiceAsync(
        Guid serviceId, AvailabilitySlotQueryParameters query, CancellationToken cancellationToken);

    Task<ServiceAvailabilitySlotDto> CreateAsync(
        Guid producerId, Guid serviceId, CreateServiceAvailabilitySlotRequest request, CancellationToken cancellationToken);

    Task<ServiceAvailabilitySlotDto> UpdateAsync(
        Guid producerId, bool isAdmin, Guid slotId, UpdateServiceAvailabilitySlotRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid producerId, bool isAdmin, Guid slotId, CancellationToken cancellationToken);
}

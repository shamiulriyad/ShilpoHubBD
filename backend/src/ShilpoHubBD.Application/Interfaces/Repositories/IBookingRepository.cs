using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<(List<Booking> Items, int TotalCount)> GetPagedByTouristAsync(
        Guid touristId, BookingQueryParameters query, CancellationToken cancellationToken);

    Task<(List<Booking> Items, int TotalCount)> GetPagedByProducerAsync(
        Guid producerId, BookingQueryParameters query, CancellationToken cancellationToken);

    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<int> GetActiveCountForSlotAsync(Guid slotId, CancellationToken cancellationToken);
    Task<Dictionary<Guid, int>> GetActiveCountsForSlotsAsync(IEnumerable<Guid> slotIds, CancellationToken cancellationToken);
    Task<bool> HasActiveBookingsForSlotAsync(Guid slotId, CancellationToken cancellationToken);
    Task AddAsync(Booking booking, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

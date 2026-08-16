using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.TouristBooking;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(Guid touristId, CreateBookingRequest request, CancellationToken cancellationToken);

    Task<PagedResult<BookingDto>> GetMyBookingsAsync(Guid touristId, BookingQueryParameters query, CancellationToken cancellationToken);

    Task<PagedResult<BookingDto>> GetProviderBookingsAsync(Guid producerId, BookingQueryParameters query, CancellationToken cancellationToken);

    Task<BookingDto> GetByIdAsync(Guid userId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task<BookingDto> ConfirmAsync(Guid producerId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task<BookingDto> RejectAsync(Guid producerId, bool isAdmin, Guid id, CancelBookingRequest request, CancellationToken cancellationToken);

    Task<BookingDto> CancelAsync(Guid userId, bool isAdmin, Guid id, CancelBookingRequest request, CancellationToken cancellationToken);

    Task<BookingDto> CompleteAsync(Guid producerId, bool isAdmin, Guid id, CancellationToken cancellationToken);
}

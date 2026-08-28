using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.Services.TouristBooking;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ITouristServiceRepository _serviceRepository;
    private readonly IServiceAvailabilitySlotRepository _slotRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        ITouristServiceRepository serviceRepository,
        IServiceAvailabilitySlotRepository slotRepository)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _slotRepository = slotRepository;
    }

    public async Task<BookingDto> CreateAsync(Guid touristId, CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken)
            ?? throw new NotFoundException("Tourist service not found.");

        if (!service.IsActive)
        {
            throw new ConflictException("This service is not currently available for booking.");
        }

        if (service.ProducerId == touristId)
        {
            throw new ConflictException("You cannot book your own service.");
        }

        var slot = await _slotRepository.GetByIdAsync(request.AvailabilitySlotId, cancellationToken)
            ?? throw new NotFoundException("Availability slot not found.");

        if (slot.ServiceId != service.Id)
        {
            throw new ConflictException("The selected slot does not belong to this service.");
        }

        if (!slot.IsActive)
        {
            throw new ConflictException("This slot is no longer available.");
        }

        if (slot.StartAt <= DateTime.UtcNow)
        {
            throw new ConflictException("You cannot book a slot that has already started.");
        }

        var bookedCount = await _bookingRepository.GetActiveCountForSlotAsync(slot.Id, cancellationToken);
        if (bookedCount + request.PartySize > slot.Capacity)
        {
            throw new ConflictException("This slot does not have enough remaining capacity.");
        }

        var now = DateTime.UtcNow;
        var booking = new Domain.Entities.TouristBooking.Booking
        {
            Id = Guid.NewGuid(),
            ServiceId = service.Id,
            AvailabilitySlotId = slot.Id,
            TouristId = touristId,
            ProducerId = service.ProducerId,
            PartySize = request.PartySize,
            TotalPrice = service.Price * request.PartySize,
            Status = BookingStatus.Pending,
            Notes = request.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _bookingRepository.AddAsync(booking, cancellationToken);
        await _bookingRepository.SaveChangesAsync(cancellationToken);

        var created = await _bookingRepository.GetByIdAsync(booking.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<PagedResult<BookingDto>> GetMyBookingsAsync(
        Guid touristId, BookingQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _bookingRepository.GetPagedByTouristAsync(touristId, query, cancellationToken);
        return new PagedResult<BookingDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PagedResult<BookingDto>> GetProviderBookingsAsync(
        Guid producerId, BookingQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _bookingRepository.GetPagedByProducerAsync(producerId, query, cancellationToken);
        return new PagedResult<BookingDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<BookingDto> GetByIdAsync(Guid userId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Booking not found.");

        if (!isAdmin && booking.TouristId != userId && booking.ProducerId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this booking.");
        }

        return ToDto(booking);
    }

    public async Task<BookingDto> ConfirmAsync(Guid producerId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var booking = await GetOwnedByProducerAsync(producerId, isAdmin, id, cancellationToken);

        if (booking.Status != BookingStatus.Pending)
        {
            throw new ConflictException("Only pending bookings can be confirmed.");
        }

        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync(cancellationToken);
        return ToDto(booking);
    }

    public async Task<BookingDto> RejectAsync(
        Guid producerId, bool isAdmin, Guid id, CancelBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await GetOwnedByProducerAsync(producerId, isAdmin, id, cancellationToken);

        if (booking.Status != BookingStatus.Pending)
        {
            throw new ConflictException("Only pending bookings can be rejected.");
        }

        booking.Status = BookingStatus.Rejected;
        booking.CancellationReason = request.Reason?.Trim();
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync(cancellationToken);
        return ToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(
        Guid userId, bool isAdmin, Guid id, CancelBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Booking not found.");

        if (!isAdmin && booking.TouristId != userId && booking.ProducerId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this booking.");
        }

        if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
        {
            throw new ConflictException("Only pending or confirmed bookings can be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = request.Reason?.Trim();
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync(cancellationToken);
        return ToDto(booking);
    }

    public async Task<BookingDto> CompleteAsync(Guid producerId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var booking = await GetOwnedByProducerAsync(producerId, isAdmin, id, cancellationToken);

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new ConflictException("Only confirmed bookings can be marked as completed.");
        }

        booking.Status = BookingStatus.Completed;
        booking.CompletedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync(cancellationToken);
        return ToDto(booking);
    }

    private async Task<Domain.Entities.TouristBooking.Booking> GetOwnedByProducerAsync(
        Guid producerId, bool isAdmin, Guid id, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Booking not found.");

        if (!isAdmin && booking.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this booking.");
        }

        return booking;
    }

    private static BookingDto ToDto(Domain.Entities.TouristBooking.Booking booking) => new()
    {
        Id = booking.Id,
        ServiceId = booking.ServiceId,
        ServiceTitle = booking.Service.Title,
        ServiceType = booking.Service.Type.ToString(),
        AvailabilitySlotId = booking.AvailabilitySlotId,
        SlotStartAt = booking.AvailabilitySlot.StartAt,
        SlotEndAt = booking.AvailabilitySlot.EndAt,
        TouristId = booking.TouristId,
        TouristName = booking.Tourist.FullName,
        ProducerId = booking.ProducerId,
        ProducerName = booking.Producer.FullName,
        PartySize = booking.PartySize,
        TotalPrice = booking.TotalPrice,
        Status = booking.Status.ToString(),
        Notes = booking.Notes,
        CancellationReason = booking.CancellationReason,
        ConfirmedAt = booking.ConfirmedAt,
        CancelledAt = booking.CancelledAt,
        CompletedAt = booking.CompletedAt,
        CreatedAt = booking.CreatedAt,
        UpdatedAt = booking.UpdatedAt,
    };
}

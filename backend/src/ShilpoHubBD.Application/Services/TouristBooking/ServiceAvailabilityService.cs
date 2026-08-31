using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.Services.TouristBooking;

public class ServiceAvailabilityService : IServiceAvailabilityService
{
    private readonly IServiceAvailabilitySlotRepository _slotRepository;
    private readonly ITouristServiceRepository _serviceRepository;
    private readonly IBookingRepository _bookingRepository;

    public ServiceAvailabilityService(
        IServiceAvailabilitySlotRepository slotRepository,
        ITouristServiceRepository serviceRepository,
        IBookingRepository bookingRepository)
    {
        _slotRepository = slotRepository;
        _serviceRepository = serviceRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<PagedResult<ServiceAvailabilitySlotDto>> GetPagedByServiceAsync(
        Guid serviceId, AvailabilitySlotQueryParameters query, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken)
            ?? throw new NotFoundException("Tourist service not found.");

        var (items, totalCount) = await _slotRepository.GetPagedByServiceAsync(serviceId, query, cancellationToken);
        var bookedCounts = await _bookingRepository.GetActiveCountsForSlotsAsync(items.Select(s => s.Id), cancellationToken);

        return new PagedResult<ServiceAvailabilitySlotDto>
        {
            Items = items.Select(s => ToDto(s, service.Title, bookedCounts.GetValueOrDefault(s.Id))).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ServiceAvailabilitySlotDto> CreateAsync(
        Guid producerId, Guid serviceId, CreateServiceAvailabilitySlotRequest request, CancellationToken cancellationToken)
    {
        var service = await GetOwnedServiceAsync(producerId, serviceId, cancellationToken);

        if (request.EndAt <= request.StartAt)
        {
            throw new ConflictException("End time must be after the start time.");
        }

        var now = DateTime.UtcNow;
        var slot = new ServiceAvailabilitySlot
        {
            Id = Guid.NewGuid(),
            ServiceId = service.Id,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Capacity = request.Capacity ?? service.DefaultCapacity,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _slotRepository.AddAsync(slot, cancellationToken);
        await _slotRepository.SaveChangesAsync(cancellationToken);

        return ToDto(slot, service.Title, 0);
    }

    public async Task<ServiceAvailabilitySlotDto> UpdateAsync(
        Guid producerId, bool isAdmin, Guid slotId, UpdateServiceAvailabilitySlotRequest request, CancellationToken cancellationToken)
    {
        var slot = await GetOwnedSlotAsync(producerId, isAdmin, slotId, cancellationToken);

        if (request.EndAt <= request.StartAt)
        {
            throw new ConflictException("End time must be after the start time.");
        }

        var bookedCount = await _bookingRepository.GetActiveCountForSlotAsync(slot.Id, cancellationToken);
        if (request.Capacity < bookedCount)
        {
            throw new ConflictException("Capacity cannot be lower than the number of people already booked into this slot.");
        }

        slot.StartAt = request.StartAt;
        slot.EndAt = request.EndAt;
        slot.Capacity = request.Capacity;
        slot.IsActive = request.IsActive;
        slot.UpdatedAt = DateTime.UtcNow;

        await _slotRepository.SaveChangesAsync(cancellationToken);

        return ToDto(slot, slot.Service.Title, bookedCount);
    }

    public async Task DeleteAsync(Guid producerId, bool isAdmin, Guid slotId, CancellationToken cancellationToken)
    {
        var slot = await GetOwnedSlotAsync(producerId, isAdmin, slotId, cancellationToken);

        if (await _bookingRepository.HasActiveBookingsForSlotAsync(slot.Id, cancellationToken))
        {
            throw new ConflictException("This slot has active bookings and cannot be deleted.");
        }

        _slotRepository.Remove(slot);
        await _slotRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Domain.Entities.TouristBooking.TouristService> GetOwnedServiceAsync(
        Guid producerId, Guid serviceId, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken)
            ?? throw new NotFoundException("Tourist service not found.");

        if (service.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this service.");
        }

        return service;
    }

    private async Task<ServiceAvailabilitySlot> GetOwnedSlotAsync(
        Guid producerId, bool isAdmin, Guid slotId, CancellationToken cancellationToken)
    {
        var slot = await _slotRepository.GetByIdAsync(slotId, cancellationToken)
            ?? throw new NotFoundException("Availability slot not found.");

        if (!isAdmin && slot.Service.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this slot.");
        }

        return slot;
    }

    private static ServiceAvailabilitySlotDto ToDto(ServiceAvailabilitySlot slot, string serviceTitle, int bookedCount) => new()
    {
        Id = slot.Id,
        ServiceId = slot.ServiceId,
        ServiceTitle = serviceTitle,
        StartAt = slot.StartAt,
        EndAt = slot.EndAt,
        Capacity = slot.Capacity,
        BookedCount = bookedCount,
        RemainingCapacity = Math.Max(0, slot.Capacity - bookedCount),
        IsActive = slot.IsActive,
        CreatedAt = slot.CreatedAt,
        UpdatedAt = slot.UpdatedAt,
    };
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Repositories;

public class ServiceAvailabilitySlotRepository : IServiceAvailabilitySlotRepository
{
    private readonly ShilpoHubDbContext _context;

    public ServiceAvailabilitySlotRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<(List<ServiceAvailabilitySlot> Items, int TotalCount)> GetPagedByServiceAsync(
        Guid serviceId, AvailabilitySlotQueryParameters query, CancellationToken cancellationToken)
    {
        var activeBookedCounts = _context.Bookings
            .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
            .GroupBy(b => b.AvailabilitySlotId)
            .Select(g => new { SlotId = g.Key, BookedCount = g.Sum(b => b.PartySize) });

        var slots = _context.ServiceAvailabilitySlots
            .Where(a => a.ServiceId == serviceId && a.IsActive)
            .GroupJoin(activeBookedCounts, a => a.Id, c => c.SlotId, (a, counts) => new { Slot = a, Counts = counts })
            .SelectMany(x => x.Counts.DefaultIfEmpty(), (x, c) => new { x.Slot, BookedCount = c == null ? 0 : c.BookedCount });

        if (query.From.HasValue)
        {
            slots = slots.Where(x => x.Slot.EndAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            slots = slots.Where(x => x.Slot.StartAt <= query.To.Value);
        }

        if (query.OnlyAvailable)
        {
            slots = slots.Where(x => x.BookedCount < x.Slot.Capacity);
        }

        var ordered = slots.OrderBy(x => x.Slot.StartAt);

        var totalCount = await ordered.CountAsync(cancellationToken);
        var items = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => x.Slot)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<ServiceAvailabilitySlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ServiceAvailabilitySlots.Include(a => a.Service).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(ServiceAvailabilitySlot slot, CancellationToken cancellationToken)
        => await _context.ServiceAvailabilitySlots.AddAsync(slot, cancellationToken);

    public void Remove(ServiceAvailabilitySlot slot)
        => _context.ServiceAvailabilitySlots.Remove(slot);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

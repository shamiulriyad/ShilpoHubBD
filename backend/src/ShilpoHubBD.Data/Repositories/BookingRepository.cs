using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ShilpoHubDbContext _context;

    public BookingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Booking> WithDetails()
        => _context.Bookings
            .Include(b => b.Service)
            .Include(b => b.AvailabilitySlot)
            .Include(b => b.Tourist)
            .Include(b => b.Producer)
            .AsSplitQuery();

    private static IQueryable<Booking> ApplyFilters(IQueryable<Booking> bookings, BookingQueryParameters query)
    {
        if (query.Status.HasValue)
        {
            bookings = bookings.Where(b => b.Status == query.Status.Value);
        }

        if (query.Type.HasValue)
        {
            bookings = bookings.Where(b => b.Service.Type == query.Type.Value);
        }

        if (query.From.HasValue)
        {
            bookings = bookings.Where(b => b.AvailabilitySlot.StartAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            bookings = bookings.Where(b => b.AvailabilitySlot.StartAt <= query.To.Value);
        }

        return bookings;
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedByTouristAsync(
        Guid touristId, BookingQueryParameters query, CancellationToken cancellationToken)
    {
        var bookings = ApplyFilters(WithDetails().Where(b => b.TouristId == touristId), query)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await bookings.CountAsync(cancellationToken);
        var items = await bookings
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedByProducerAsync(
        Guid producerId, BookingQueryParameters query, CancellationToken cancellationToken)
    {
        var bookings = ApplyFilters(WithDetails().Where(b => b.ProducerId == producerId), query)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await bookings.CountAsync(cancellationToken);
        var items = await bookings
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<int> GetActiveCountForSlotAsync(Guid slotId, CancellationToken cancellationToken)
        => _context.Bookings
            .Where(b => b.AvailabilitySlotId == slotId
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
            .SumAsync(b => b.PartySize, cancellationToken);

    public async Task<Dictionary<Guid, int>> GetActiveCountsForSlotsAsync(IEnumerable<Guid> slotIds, CancellationToken cancellationToken)
    {
        var ids = slotIds.ToList();
        return await _context.Bookings
            .Where(b => ids.Contains(b.AvailabilitySlotId)
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
            .GroupBy(b => b.AvailabilitySlotId)
            .Select(g => new { SlotId = g.Key, BookedCount = g.Sum(b => b.PartySize) })
            .ToDictionaryAsync(x => x.SlotId, x => x.BookedCount, cancellationToken);
    }

    public Task<bool> HasActiveBookingsForSlotAsync(Guid slotId, CancellationToken cancellationToken)
        => _context.Bookings.AnyAsync(b => b.AvailabilitySlotId == slotId
            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed), cancellationToken);

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
        => await _context.Bookings.AddAsync(booking, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

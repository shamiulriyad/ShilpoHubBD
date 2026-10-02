using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Domain.Entities.Passport;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Seed;

/// <summary>
/// Adds an idempotent, presentation-ready travel history for the shared tourist demo account.
/// Existing user activity is never changed or removed.
/// </summary>
public static class TouristDemoSeeder
{
    private const string TouristEmail = "tourist@gmail.com";
    private const string BookingMarker = "[TOURIST-DEMO]";

    public static async Task SeedAsync(ShilpoHubDbContext context, CancellationToken cancellationToken = default)
    {
        var tourist = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == TouristEmail, cancellationToken);
        if (tourist is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var places = await context.HeritagePlaces
            .Where(p => p.IsActive)
            .GroupBy(p => p.DistrictId)
            .Select(group => group.OrderBy(p => p.Name).First())
            .OrderBy(p => p.Name)
            .Take(7)
            .ToListAsync(cancellationToken);

        var existingPlaceIds = await context.HeritageCheckIns
            .Where(c => c.UserId == tourist.Id)
            .Select(c => c.HeritagePlaceId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var visitIndex = 0;
        foreach (var place in places.Where(p => !existingPlaceIds.Contains(p.Id)))
        {
            var visitedAt = now.AddDays(-14 * (7 - visitIndex++)).AddHours(-3);
            context.HeritageCheckIns.Add(new HeritageCheckIn
            {
                Id = Guid.NewGuid(),
                UserId = tourist.Id,
                HeritagePlaceId = place.Id,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                CheckInDate = DateOnly.FromDateTime(visitedAt),
                CheckedInAt = visitedAt,
            });
        }
        await context.SaveChangesAsync(cancellationToken);

        if (await context.Bookings.AnyAsync(b => b.TouristId == tourist.Id && b.Notes != null && b.Notes.Contains(BookingMarker), cancellationToken))
        {
            return;
        }

        var services = await context.TouristServices
            .Include(s => s.AvailabilitySlots)
            .Where(s => s.IsActive && s.AvailabilitySlots.Any())
            .OrderBy(s => s.Type)
            .ToListAsync(cancellationToken);
        if (services.Count == 0)
        {
            return;
        }

        var uniqueSiteCount = await context.HeritageCheckIns
            .Where(c => c.UserId == tourist.Id)
            .Select(c => c.HeritagePlaceId)
            .Distinct()
            .CountAsync(cancellationToken);
        var tier = ExplorerTierPolicy.Resolve(uniqueSiteCount);

        var selected = services
            .OrderByDescending(s => s.Type == BookingType.HomestayBooking)
            .ThenBy(s => s.Title)
            .Take(3)
            .ToList();
        var statuses = new[] { BookingStatus.Confirmed, BookingStatus.Completed, BookingStatus.Pending };

        for (var index = 0; index < selected.Count; index++)
        {
            var service = selected[index];
            var partySize = index == 0 ? 2 : 1;
            var basePrice = service.Price * partySize;
            var discount = service.Type == BookingType.HomestayBooking ? tier.DiscountPercent : 0m;
            var status = statuses[index];
            var startAt = status == BookingStatus.Completed ? now.AddDays(-30) : now.AddDays(7 + index * 4);
            var slot = new ServiceAvailabilitySlot
            {
                Id = Guid.NewGuid(), ServiceId = service.Id, StartAt = startAt,
                EndAt = startAt.AddMinutes(service.DurationMinutes ?? 24 * 60),
                Capacity = Math.Max(service.DefaultCapacity, partySize), IsActive = status != BookingStatus.Completed,
                CreatedAt = now.AddDays(-40), UpdatedAt = now,
            };
            context.ServiceAvailabilitySlots.Add(slot);

            context.Bookings.Add(new Booking
            {
                Id = Guid.NewGuid(), ServiceId = service.Id, AvailabilitySlotId = slot.Id,
                TouristId = tourist.Id, ProducerId = service.ProducerId, PartySize = partySize,
                TotalPrice = decimal.Round(basePrice * (1m - discount / 100m), 2), Status = status,
                Notes = $"{BookingMarker} Curated demo itinerary",
                ConfirmedAt = status is BookingStatus.Confirmed or BookingStatus.Completed ? now.AddDays(-8 + index) : null,
                CompletedAt = status == BookingStatus.Completed ? now.AddDays(-2) : null,
                CreatedAt = now.AddDays(-10 + index), UpdatedAt = now.AddDays(-2 + index),
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

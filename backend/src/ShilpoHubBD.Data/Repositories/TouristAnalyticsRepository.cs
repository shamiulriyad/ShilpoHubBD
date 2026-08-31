using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.TouristAnalytics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Passport;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Repositories;

public class TouristAnalyticsRepository : ITouristAnalyticsRepository
{
    private readonly ShilpoHubDbContext _context;

    public TouristAnalyticsRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<VisitedLocationDto>> GetVisitedLocationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.HeritageCheckIns
            .Where(c => c.UserId == userId)
            .GroupBy(c => new { c.HeritagePlaceId, c.HeritagePlace.Name, DistrictName = c.HeritagePlace.District.Name })
            .Select(g => new VisitedLocationDto
            {
                HeritagePlaceId = g.Key.HeritagePlaceId,
                HeritagePlaceName = g.Key.Name,
                DistrictName = g.Key.DistrictName,
                VisitCount = g.Count(),
                FirstVisitedAt = g.Min(c => c.CheckedInAt),
                LastVisitedAt = g.Max(c => c.CheckedInAt),
            })
            .OrderByDescending(v => v.LastVisitedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PopularDestinationDto>> GetPopularDestinationsAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.HeritageCheckIns
            .GroupBy(c => new { c.HeritagePlaceId, c.HeritagePlace.Name })
            .Select(g => new PopularDestinationDto
            {
                HeritagePlaceId = g.Key.HeritagePlaceId,
                HeritagePlaceName = g.Key.Name,
                VisitCount = g.Count(),
            })
            .OrderByDescending(d => d.VisitCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<BookingStatisticsDto> GetBookingStatisticsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var bookings = _context.Bookings.Where(b => b.TouristId == userId);

        var totalBookings = await bookings.CountAsync(cancellationToken);
        var completedBookings = await bookings.CountAsync(b => b.Status == BookingStatus.Completed, cancellationToken);
        var cancelledBookings = await bookings.CountAsync(
            b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Rejected || b.Status == BookingStatus.NoShow,
            cancellationToken);
        var pendingBookings = await bookings.CountAsync(
            b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed,
            cancellationToken);
        var totalSpent = await bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .SumAsync(b => b.TotalPrice, cancellationToken);

        return new BookingStatisticsDto
        {
            TotalBookings = totalBookings,
            CompletedBookings = completedBookings,
            CancelledBookings = cancelledBookings,
            PendingBookings = pendingBookings,
            TotalSpent = totalSpent,
        };
    }

    public async Task<List<TravelSpendingByMonthDto>> GetTravelSpendingByMonthAsync(Guid userId, int months, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        return await _context.Bookings
            .Where(b => b.TouristId == userId && b.Status == BookingStatus.Completed && b.CreatedAt >= cutoff)
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new TravelSpendingByMonthDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalSpent = g.Sum(b => b.TotalPrice),
                BookingCount = g.Count(),
            })
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FavoriteBookingCategoryDto>> GetFavoriteBookingCategoriesAsync(Guid userId, int count, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .Where(b => b.TouristId == userId)
            .GroupBy(b => b.Service.Type)
            .Select(g => new FavoriteBookingCategoryDto
            {
                BookingType = g.Key.ToString(),
                BookingCount = g.Count(),
                TotalSpent = g.Sum(b => b.TotalPrice),
            })
            .OrderByDescending(c => c.BookingCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<FestivalParticipationDto> GetFestivalParticipationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var festivalNames = await _context.UserBadges
            .Where(ub => ub.UserId == userId && ub.Badge.Type == BadgeType.Festival)
            .Select(ub => ub.Badge.FestivalName!)
            .ToListAsync(cancellationToken);

        return new FestivalParticipationDto
        {
            FestivalBadgeCount = festivalNames.Count,
            FestivalNames = festivalNames,
        };
    }

    public async Task<DistrictCoverageDto> GetDistrictCoverageAsync(Guid userId, CancellationToken cancellationToken)
    {
        var visitedDistrictNames = await _context.HeritageCheckIns
            .Where(c => c.UserId == userId)
            .Select(c => c.HeritagePlace.District.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var totalDistrictCount = await _context.Districts.CountAsync(cancellationToken);

        return new DistrictCoverageDto
        {
            VisitedDistrictCount = visitedDistrictNames.Count,
            TotalDistrictCount = totalDistrictCount,
            VisitedDistrictNames = visitedDistrictNames,
        };
    }

    public async Task<Dictionary<string, int>> GetBadgeCountsByTypeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var counts = await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .GroupBy(ub => ub.Badge.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(c => c.Type.ToString(), c => c.Count);
    }
}

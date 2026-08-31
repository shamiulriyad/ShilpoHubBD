using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageCheckInRepository : IHeritageCheckInRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageCheckInRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<HeritageCheckIn>> GetMyCheckInsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.HeritageCheckIns
            .Include(c => c.HeritagePlace)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CheckedInAt)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsForDateAsync(Guid userId, Guid heritagePlaceId, DateOnly checkInDate, CancellationToken cancellationToken)
        => _context.HeritageCheckIns.AnyAsync(
            c => c.UserId == userId && c.HeritagePlaceId == heritagePlaceId && c.CheckInDate == checkInDate,
            cancellationToken);

    public Task<bool> HasCheckedInAsync(Guid userId, Guid heritagePlaceId, CancellationToken cancellationToken)
        => _context.HeritageCheckIns.AnyAsync(
            c => c.UserId == userId && c.HeritagePlaceId == heritagePlaceId,
            cancellationToken);

    public Task<int> GetCheckInCountAsync(Guid userId, CancellationToken cancellationToken)
        => _context.HeritageCheckIns.CountAsync(c => c.UserId == userId, cancellationToken);

    public Task<int> GetDistinctVisitedPlaceCountAsync(Guid userId, CancellationToken cancellationToken)
        => _context.HeritageCheckIns
            .Where(c => c.UserId == userId)
            .Select(c => c.HeritagePlaceId)
            .Distinct()
            .CountAsync(cancellationToken);

    public Task<List<Guid>> GetDistinctVisitedDistrictIdsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.HeritageCheckIns
            .Where(c => c.UserId == userId)
            .Select(c => c.HeritagePlace.DistrictId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task AddAsync(HeritageCheckIn checkIn, CancellationToken cancellationToken)
        => await _context.HeritageCheckIns.AddAsync(checkIn, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

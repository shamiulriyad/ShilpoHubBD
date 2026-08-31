using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritageCheckInRepository
{
    Task<List<HeritageCheckIn>> GetMyCheckInsAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ExistsForDateAsync(Guid userId, Guid heritagePlaceId, DateOnly checkInDate, CancellationToken cancellationToken);
    Task<bool> HasCheckedInAsync(Guid userId, Guid heritagePlaceId, CancellationToken cancellationToken);
    Task<int> GetCheckInCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> GetDistinctVisitedPlaceCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<Guid>> GetDistinctVisitedDistrictIdsAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(HeritageCheckIn checkIn, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> NidInUseAsync(string nidNumber, Guid exceptUserId, CancellationToken cancellationToken);
    Task<List<string>> GetProducerExpertiseOptionsAsync(CancellationToken cancellationToken);
    Task<bool> IsApprovedAsync(Guid userId, CancellationToken cancellationToken);
    Task<(List<UserProfile> Items, int TotalCount)> GetPagedAsync(UserProfileQueryParameters query, CancellationToken cancellationToken);
    Task<Dictionary<Guid, List<string>>> GetRolesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
    Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken);
    Task AddAsync(UserProfile profile, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

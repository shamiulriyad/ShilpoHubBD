using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Domain.Entities.Admin;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IIdentityVerificationRepository
{
    Task AddAsync(IdentityVerificationRequest request, CancellationToken cancellationToken);

    Task<IdentityVerificationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<IdentityVerificationRequest> Items, int TotalCount)> GetPagedAsync(
        IdentityVerificationQueryParameters query, CancellationToken cancellationToken);

    Task<List<IdentityVerificationRequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Dictionary<Guid, IdentityVerificationStatus>> GetLatestStatusesByUserIdsAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken);

    Task<bool> HasPendingRequestAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

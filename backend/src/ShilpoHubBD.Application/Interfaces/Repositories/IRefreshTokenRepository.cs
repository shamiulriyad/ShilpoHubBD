using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);
    Task RevokeAllActiveForUserAsync(Guid userId, string? revokedByIp, string reason, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IThreatDetectionRepository
{
    Task AddLoginAttemptAsync(LoginAttempt attempt, CancellationToken cancellationToken);
    Task<bool> IsIpBlockedAsync(string ipAddress, CancellationToken cancellationToken);
    Task<(List<LoginAttempt> Items, int TotalCount)> GetFailedLoginsPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken);
    Task<List<(string IpAddress, int FailedCount)>> GetSuspiciousIpsAsync(
        DateTime since, int minFailedAttempts, CancellationToken cancellationToken);
    Task<List<BlockedIpAddress>> GetBlockedIpsAsync(CancellationToken cancellationToken);
    Task<BlockedIpAddress?> GetBlockedIpByAddressAsync(string ipAddress, CancellationToken cancellationToken);
    Task AddBlockedIpAsync(BlockedIpAddress blocked, CancellationToken cancellationToken);
    void RemoveBlockedIp(BlockedIpAddress blocked);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

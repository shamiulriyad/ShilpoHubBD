using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IThreatDetectionService
{
    Task<PagedResult<LoginAttemptDto>> GetFailedLoginsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<List<SuspiciousIpDto>> GetSuspiciousIpsAsync(CancellationToken cancellationToken);
    Task<List<BlockedIpDto>> GetBlockedIpsAsync(CancellationToken cancellationToken);
    Task<BlockedIpDto> BlockIpAsync(Guid userId, BlockIpRequest request, CancellationToken cancellationToken);
    Task UnblockIpAsync(string ipAddress, CancellationToken cancellationToken);
}

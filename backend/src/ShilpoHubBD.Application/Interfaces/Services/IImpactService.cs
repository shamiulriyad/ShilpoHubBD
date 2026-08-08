using ShilpoHubBD.Application.DTOs.Impact;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IImpactService
{
    Task<ImpactSummaryDto> GetMyImpactAsync(Guid userId, CancellationToken cancellationToken);
}

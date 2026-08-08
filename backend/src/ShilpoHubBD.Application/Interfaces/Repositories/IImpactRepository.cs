using ShilpoHubBD.Application.DTOs.Impact;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IImpactRepository
{
    Task<ImpactStatsDto> GetImpactStatsAsync(Guid userId, CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IGovForecastService
{
    Task<GovForecastDto> GenerateAsync(
        Guid userId, GenerateGovForecastRequest request, CancellationToken cancellationToken);

    Task<PagedResult<GovForecastListItemDto>> GetForecastsAsync(
        GovForecastQueryParameters query, CancellationToken cancellationToken);

    Task<GovForecastDto> GetForecastByIdAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteForecastAsync(Guid id, CancellationToken cancellationToken);
}

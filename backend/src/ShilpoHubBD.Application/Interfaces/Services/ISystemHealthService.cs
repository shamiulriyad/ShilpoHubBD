using ShilpoHubBD.Application.DTOs.Security;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISystemHealthService
{
    Task<SystemHealthDto> GetHealthAsync(CancellationToken cancellationToken);
}

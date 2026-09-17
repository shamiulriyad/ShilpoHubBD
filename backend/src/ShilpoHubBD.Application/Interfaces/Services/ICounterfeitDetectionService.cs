using ShilpoHubBD.Application.DTOs.CounterfeitDetection;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICounterfeitDetectionService
{
    Task<CounterfeitCheckResultDto> CheckAsync(Guid productId, CancellationToken cancellationToken);
}

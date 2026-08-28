using ShilpoHubBD.Application.DTOs.ProducerComparison;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProducerComparisonService
{
    Task<List<ProducerComparisonRowDto>> CompareAsync(ProducerComparisonRequest request, CancellationToken cancellationToken);
}

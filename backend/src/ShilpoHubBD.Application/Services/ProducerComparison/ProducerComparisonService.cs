using ShilpoHubBD.Application.DTOs.ProducerComparison;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.ProducerComparison;

public class ProducerComparisonService : IProducerComparisonService
{
    private readonly IProducerComparisonRepository _producerComparisonRepository;

    public ProducerComparisonService(IProducerComparisonRepository producerComparisonRepository)
    {
        _producerComparisonRepository = producerComparisonRepository;
    }

    public async Task<List<ProducerComparisonRowDto>> CompareAsync(ProducerComparisonRequest request, CancellationToken cancellationToken)
    {
        var requestedIds = request.ProducerIds.Distinct().ToList();
        var rows = await _producerComparisonRepository.CompareAsync(requestedIds, cancellationToken);

        var foundIds = rows.Select(r => r.ProducerId).ToHashSet();
        var missingIds = requestedIds.Where(id => !foundIds.Contains(id)).ToList();
        if (missingIds.Count > 0)
        {
            throw new NotFoundException($"Producer(s) not found: {string.Join(", ", missingIds)}");
        }

        return rows;
    }
}

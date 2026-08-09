using ShilpoHubBD.Application.DTOs.ProducerComparison;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerComparisonRepository
{
    // Returns one row per id in producerIds that is a valid, existing Producer; invalid/missing ids
    // are simply omitted so the caller can diff the result against the request to report them.
    Task<List<ProducerComparisonRowDto>> CompareAsync(List<Guid> producerIds, CancellationToken cancellationToken);
}

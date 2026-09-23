using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ITransportOptionRepository
{
    // Active options for one mode that serve the destination district, from any origin the caller
    // then filters against the traveller's free-text starting point.
    Task<List<TransportOption>> GetForDestinationAsync(string destinationDistrict, string mode, CancellationToken cancellationToken);
}

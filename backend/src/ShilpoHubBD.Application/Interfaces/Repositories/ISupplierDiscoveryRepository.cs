using ShilpoHubBD.Application.DTOs.SupplierDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ISupplierDiscoveryRepository
{
    Task<(List<SupplierSearchResultDto> Items, int TotalCount)> SearchAsync(SupplierSearchParameters parameters, CancellationToken cancellationToken);
    Task<SupplierProfileDto?> GetProducerProfileAsync(Guid producerId, CancellationToken cancellationToken);
}

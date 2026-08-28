using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISupplierDiscoveryService
{
    Task<PagedResult<SupplierSearchResultDto>> SearchAsync(SupplierSearchParameters parameters, CancellationToken cancellationToken);
    Task<SupplierProfileDto> GetProducerProfileAsync(Guid producerId, CancellationToken cancellationToken);
}

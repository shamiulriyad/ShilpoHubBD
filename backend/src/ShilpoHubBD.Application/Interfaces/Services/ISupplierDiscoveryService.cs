using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISupplierDiscoveryService
{
    Task<PagedResult<SupplierSearchResultDto>> SearchAsync(SupplierSearchParameters parameters, CancellationToken cancellationToken);
    Task<SupplierProfileDto> GetProducerProfileAsync(Guid producerId, CancellationToken cancellationToken);

    /// <summary>Partnership-evaluation view: revenue, orders, growth, best sellers, repeat-customer rate. No customer PII.</summary>
    Task<ProducerBusinessProfileDto> GetBusinessProfileAsync(Guid producerId, CancellationToken cancellationToken);
}

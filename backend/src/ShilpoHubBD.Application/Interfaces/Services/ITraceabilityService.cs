using ShilpoHubBD.Application.DTOs.Traceability;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ITraceabilityService
{
    Task<ProductTraceabilityDto> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task<ProductTraceabilityDto> CreateAsync(Guid producerId, CreateProductTraceabilityRequest request, CancellationToken cancellationToken);
    Task<ProductTraceabilityDto> UpdateAsync(Guid id, Guid producerId, bool isAdmin, UpdateProductTraceabilityRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);
}

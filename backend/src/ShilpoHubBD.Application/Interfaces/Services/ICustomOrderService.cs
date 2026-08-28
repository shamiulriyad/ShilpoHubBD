using ShilpoHubBD.Application.DTOs.CustomOrders;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICustomOrderService
{
    Task<CustomOrderRequestDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<List<CustomOrderRequestDto>> GetMineAsCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task<List<CustomOrderRequestDto>> GetMineAsProducerAsync(Guid producerId, CancellationToken cancellationToken);
    Task<CustomOrderRequestDto> CreateAsync(Guid customerId, CreateCustomOrderRequest request, CancellationToken cancellationToken);
    Task<CustomOrderRequestDto> RespondAsync(Guid id, Guid producerId, bool isAdmin, RespondToCustomOrderRequest request, CancellationToken cancellationToken);
    Task<CustomOrderRequestDto> CancelAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
}

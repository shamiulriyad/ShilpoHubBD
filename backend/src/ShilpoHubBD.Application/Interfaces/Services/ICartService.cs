using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICartService
{
    Task<List<CartItemDto>> GetCartAsync(Guid userId, CancellationToken cancellationToken);
    Task<CartItemDto> AddOrIncrementAsync(Guid userId, Guid productId, Guid? productVariantId, int quantity, CancellationToken cancellationToken);
    Task<CartItemDto> UpdateQuantityAsync(Guid userId, Guid cartItemId, int quantity, CancellationToken cancellationToken);
    Task RemoveItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken);
    Task ClearCartAsync(Guid userId, CancellationToken cancellationToken);
    Task<CartSummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken);
}

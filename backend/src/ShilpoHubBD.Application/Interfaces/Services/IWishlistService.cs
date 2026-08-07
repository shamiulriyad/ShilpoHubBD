using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IWishlistService
{
    Task<List<WishlistItemDto>> GetWishlistAsync(Guid userId, CancellationToken cancellationToken);
    Task<WishlistItemDto> AddAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task RemoveAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task<CartItemDto> MoveToCartAsync(Guid userId, Guid productId, MoveToCartRequest request, CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Services.Commerce;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICartService _cartService;

    public WishlistService(IWishlistRepository wishlistRepository, IProductRepository productRepository, ICartService cartService)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
        _cartService = cartService;
    }

    public async Task<List<WishlistItemDto>> GetWishlistAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await _wishlistRepository.GetByUserIdAsync(userId, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<WishlistItemDto> AddAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var existing = await _wishlistRepository.GetAsync(userId, productId, cancellationToken);
        if (existing is not null)
        {
            return ToDto(existing);
        }

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        var item = new WishlistItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            Product = product,
            CreatedAt = DateTime.UtcNow,
        };

        await _wishlistRepository.AddAsync(item, cancellationToken);
        await _wishlistRepository.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task RemoveAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var item = await _wishlistRepository.GetAsync(userId, productId, cancellationToken)
            ?? throw new NotFoundException("Wishlist item not found.");

        _wishlistRepository.Remove(item);
        await _wishlistRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<CartItemDto> MoveToCartAsync(Guid userId, Guid productId, MoveToCartRequest request, CancellationToken cancellationToken)
    {
        var item = await _wishlistRepository.GetAsync(userId, productId, cancellationToken)
            ?? throw new NotFoundException("Wishlist item not found.");

        var cartItem = await _cartService.AddOrIncrementAsync(userId, productId, request.ProductVariantId, request.Quantity, cancellationToken);

        _wishlistRepository.Remove(item);
        await _wishlistRepository.SaveChangesAsync(cancellationToken);

        return cartItem;
    }

    private static WishlistItemDto ToDto(WishlistItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductName = item.Product.Name,
        ProductSlug = item.Product.Slug,
        PrimaryImageUrl = item.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        Price = item.Product.Price,
        DiscountPrice = item.Product.DiscountPrice,
        IsAvailable = item.Product.IsActive && item.Product.Stock > 0,
        CreatedAt = item.CreatedAt,
    };
}

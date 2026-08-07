using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Commerce;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<List<CartItemDto>> GetCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<CartItemDto> AddOrIncrementAsync(Guid userId, Guid productId, Guid? productVariantId, int quantity, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (!product.IsActive)
        {
            throw new ConflictException("This product is no longer available.");
        }

        var variant = productVariantId.HasValue
            ? product.Variants.FirstOrDefault(v => v.Id == productVariantId.Value)
                ?? throw new NotFoundException("Variant not found for this product.")
            : null;

        var existing = await _cartRepository.GetAsync(userId, productId, productVariantId, cancellationToken);
        var now = DateTime.UtcNow;

        EnsureStockAvailable(product, variant, (existing?.Quantity ?? 0) + quantity);

        if (existing is not null)
        {
            existing.Quantity += quantity;
            existing.UpdatedAt = now;
            await _cartRepository.SaveChangesAsync(cancellationToken);
            return ToDto(existing);
        }

        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            Product = product,
            ProductVariantId = productVariantId,
            ProductVariant = variant,
            Quantity = quantity,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _cartRepository.AddAsync(item, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task<CartItemDto> UpdateQuantityAsync(Guid userId, Guid cartItemId, int quantity, CancellationToken cancellationToken)
    {
        var item = await _cartRepository.GetByIdAsync(cartItemId, cancellationToken)
            ?? throw new NotFoundException("Cart item not found.");

        EnsureOwnership(item, userId);
        EnsureStockAvailable(item.Product, item.ProductVariant, quantity);

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task RemoveItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken)
    {
        var item = await _cartRepository.GetByIdAsync(cartItemId, cancellationToken)
            ?? throw new NotFoundException("Cart item not found.");

        EnsureOwnership(item, userId);

        _cartRepository.Remove(item);
        await _cartRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken cancellationToken)
        => await _cartRepository.ClearAsync(userId, cancellationToken);

    public async Task<CartSummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);

        return new CartSummaryDto
        {
            ItemCount = items.Count,
            TotalQuantity = items.Sum(i => i.Quantity),
            Subtotal = items.Sum(i => (i.ProductVariant?.Price ?? i.Product.Price) * i.Quantity),
        };
    }

    private static void EnsureOwnership(CartItem item, Guid userId)
    {
        if (item.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this cart item.");
        }
    }

    private static void EnsureStockAvailable(Product product, ProductVariant? variant, int requestedQuantity)
    {
        var availableStock = variant?.Stock ?? product.Stock;
        if (requestedQuantity > availableStock)
        {
            throw new ConflictException($"Only {availableStock} unit(s) of this item are available.");
        }
    }

    private static CartItemDto ToDto(CartItem item)
    {
        var unitPrice = item.ProductVariant?.Price ?? item.Product.Price;

        return new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            ProductSlug = item.Product.Slug,
            PrimaryImageUrl = item.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
            ProductVariantId = item.ProductVariantId,
            VariantName = item.ProductVariant?.Name,
            UnitPrice = unitPrice,
            Quantity = item.Quantity,
            LineTotal = unitPrice * item.Quantity,
            CreatedAt = item.CreatedAt,
        };
    }
}

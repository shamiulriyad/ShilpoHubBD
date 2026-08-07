using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductQueryParameters query, CancellationToken cancellationToken);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductDto> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<List<ProductListItemDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken);
    Task<List<ProductListItemDto>> GetTrendingAsync(int count, CancellationToken cancellationToken);
    Task<List<ProductDto>> GetMineAsync(Guid producerId, CancellationToken cancellationToken);
    Task<ProductDto> CreateAsync(Guid producerId, CreateProductRequest request, CancellationToken cancellationToken);
    Task<ProductDto> UpdateAsync(Guid id, Guid currentUserId, bool isAdmin, UpdateProductRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<ProductDto> SetFeaturedAsync(Guid id, bool isFeatured, CancellationToken cancellationToken);

    Task<ProductDto> AddVariantAsync(Guid productId, Guid currentUserId, bool isAdmin, CreateProductVariantRequest request, CancellationToken cancellationToken);
    Task<ProductDto> UpdateVariantAsync(Guid productId, Guid variantId, Guid currentUserId, bool isAdmin, UpdateProductVariantRequest request, CancellationToken cancellationToken);
    Task<ProductDto> DeleteVariantAsync(Guid productId, Guid variantId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}

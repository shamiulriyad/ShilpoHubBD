using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<List<Product>> GetFeaturedAsync(int count, CancellationToken cancellationToken);
    Task<List<Product>> GetTrendingAsync(int count, CancellationToken cancellationToken);
    Task<List<Product>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task AddVariantAsync(ProductVariant variant, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

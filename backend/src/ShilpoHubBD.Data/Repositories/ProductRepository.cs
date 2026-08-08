using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProductRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Product> WithDetails()
        => _context.Products
            .Include(p => p.Category)
            .Include(p => p.District)
            .Include(p => p.Producer)
            .Include(p => p.HandmadeVerifiedBy)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Videos)
            .AsSplitQuery();

    public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query, CancellationToken cancellationToken)
    {
        var products = WithDetails().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            products = products.Where(p => EF.Functions.ILike(p.Name, term) || EF.Functions.ILike(p.Description, term));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.DistrictId.HasValue)
        {
            products = products.Where(p => p.DistrictId == query.DistrictId.Value);
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= query.MaxPrice.Value);
        }

        products = query.SortBy switch
        {
            ProductSortOption.PriceLowToHigh => products.OrderBy(p => p.Price),
            ProductSortOption.PriceHighToLow => products.OrderByDescending(p => p.Price),
            ProductSortOption.Popular => products.OrderByDescending(p => p.ViewCount + p.SalesCount * 5),
            ProductSortOption.TopRated => products.OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.ReviewCount),
            _ => products.OrderByDescending(p => p.CreatedAt),
        };

        var totalCount = await products.CountAsync(cancellationToken);

        var items = await products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

    public Task<List<Product>> GetFeaturedAsync(int count, CancellationToken cancellationToken)
        => WithDetails()
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public Task<List<Product>> GetTrendingAsync(int count, CancellationToken cancellationToken)
        => WithDetails()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.ViewCount + p.SalesCount * 5)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public Task<List<Product>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(p => p.ProducerId == producerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
        => _context.Products.AnyAsync(p => p.Slug == slug, cancellationToken);

    public Task<List<Product>> GetLowStockByProducerAsync(Guid producerId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(p => p.ProducerId == producerId && p.IsActive
                && p.LowStockThreshold.HasValue && p.Stock <= p.LowStockThreshold.Value)
            .OrderBy(p => p.Stock)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
        => await _context.Products.AddAsync(product, cancellationToken);

    public async Task AddVariantAsync(ProductVariant variant, CancellationToken cancellationToken)
        => await _context.ProductVariants.AddAsync(variant, cancellationToken);

    public async Task AddVideoAsync(ProductVideo video, CancellationToken cancellationToken)
        => await _context.ProductVideos.AddAsync(video, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

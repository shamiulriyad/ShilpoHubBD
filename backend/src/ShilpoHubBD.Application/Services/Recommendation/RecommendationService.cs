using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Recommendation;

public class RecommendationService : IRecommendationService
{
    private const int CandidatePoolSize = 200;

    private readonly IProductRepository _productRepository;
    private readonly IRecommendationProvider _recommendationProvider;

    public RecommendationService(IProductRepository productRepository, IRecommendationProvider recommendationProvider)
    {
        _productRepository = productRepository;
        _recommendationProvider = recommendationProvider;
    }

    public async Task<List<ProductListItemDto>> GetRecommendedForMeAsync(Guid? userId, int count, CancellationToken cancellationToken)
    {
        var candidates = await GetCandidatePoolAsync(cancellationToken);
        var recommended = await _recommendationProvider.RecommendForUserAsync(userId, candidates, Math.Clamp(count, 1, 50), cancellationToken);
        return recommended.Select(ToListItemDto).ToList();
    }

    public async Task<List<ProductListItemDto>> GetSimilarAsync(Guid productId, int count, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        var candidates = await GetCandidatePoolAsync(cancellationToken);
        var similar = await _recommendationProvider.RecommendSimilarAsync(product, candidates, Math.Clamp(count, 1, 50), cancellationToken);
        return similar.Select(ToListItemDto).ToList();
    }

    private async Task<List<Product>> GetCandidatePoolAsync(CancellationToken cancellationToken)
    {
        var query = new ProductQueryParameters { Page = 1, PageSize = CandidatePoolSize };
        var (items, _) = await _productRepository.GetPagedAsync(query, cancellationToken);
        return items;
    }

    private static ProductListItemDto ToListItemDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Slug = product.Slug,
        Price = product.Price,
        DiscountPrice = product.DiscountPrice,
        PrimaryImageUrl = product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        CategoryId = product.CategoryId,
        CategoryName = product.Category.Name,
        DistrictId = product.DistrictId,
        DistrictName = product.District.Name,
        ProducerId = product.ProducerId,
        ProducerName = product.Producer.FullName,
        AverageRating = product.AverageRating,
        ReviewCount = product.ReviewCount,
        IsFeatured = product.IsFeatured,
    };
}

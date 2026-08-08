using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IRecommendationService
{
    Task<List<ProductListItemDto>> GetRecommendedForMeAsync(Guid? userId, int count, CancellationToken cancellationToken);
    Task<List<ProductListItemDto>> GetSimilarAsync(Guid productId, int count, CancellationToken cancellationToken);
}

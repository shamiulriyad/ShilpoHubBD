using ShilpoHubBD.Application.DTOs.ProductIntelligence;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Business Partner product intelligence: real aggregated data first, AI insights (a separate,
/// slower call) computed only from that same aggregated data — never a second, independent
/// database read on the AI side.
/// </summary>
public interface IProductIntelligenceService
{
    Task<ProductIntelligenceDto> GetIntelligenceAsync(Guid productId, ProductIntelligenceRange range, CancellationToken cancellationToken);
    Task<ProductIntelligenceAiInsightsDto> GetAiInsightsAsync(Guid productId, ProductIntelligenceRange range, CancellationToken cancellationToken);
}

using ShilpoHubBD.Application.DTOs.ProductIntelligence;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Abstraction over the "intelligence" behind Product Intelligence AI insights. A pure function of
/// pre-fetched, already-aggregated context -> result, matching IAIBusinessProvider/IAITourismProvider,
/// so a rule-based fallback and a real Gemini implementation are interchangeable.
/// </summary>
public interface IProductIntelligenceAIProvider
{
    Task<ProductIntelligenceAiInsightsDto> GenerateInsightsAsync(ProductIntelligenceAiContext context, CancellationToken cancellationToken);
}

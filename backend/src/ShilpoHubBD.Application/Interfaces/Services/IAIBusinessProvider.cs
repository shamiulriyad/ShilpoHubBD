using ShilpoHubBD.Application.DTOs.AIBusiness;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Abstraction over the "intelligence" behind the AI Business Assistant. Every method is a pure
// function of pre-fetched context -> result, so a future Gemini/OpenAI/custom-ML implementation
// can be registered in place of DummyAIBusinessProvider without touching AIBusinessService or
// AIBusinessAssistantController.
public interface IAIBusinessProvider
{
    Task<PriceSuggestionResult> SuggestPriceAsync(PriceSuggestionContext context, CancellationToken cancellationToken);

    Task<ProductDescriptionResult> GenerateDescriptionAsync(ProductDescriptionContext context, CancellationToken cancellationToken);

    Task<BusinessTranslationResult> TranslateAsync(BusinessTranslationRequest request, CancellationToken cancellationToken);

    Task<DemandForecastResult> ForecastDemandAsync(DemandForecastContext context, CancellationToken cancellationToken);

    Task<ProductionPlanResult> PlanProductionAsync(ProductionPlannerContext context, CancellationToken cancellationToken);

    Task<MaterialForecastResult> ForecastMaterialsAsync(MaterialForecastRequest request, CancellationToken cancellationToken);

    Task<SeasonalPredictionResult> PredictSeasonalTrendAsync(SeasonalPredictionContext context, CancellationToken cancellationToken);

    Task<SalesInsightsResult> GenerateSalesInsightsAsync(SalesInsightsContext context, CancellationToken cancellationToken);
}

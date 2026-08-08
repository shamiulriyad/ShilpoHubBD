using ShilpoHubBD.Application.DTOs.AIBusiness;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAIBusinessService
{
    Task<PriceSuggestionResult> SuggestPriceAsync(Guid producerId, PriceSuggestionRequest request, CancellationToken cancellationToken);

    Task<ProductDescriptionResult> GenerateDescriptionAsync(Guid producerId, ProductDescriptionRequest request, CancellationToken cancellationToken);

    Task<BusinessTranslationResult> TranslateAsync(BusinessTranslationRequest request, CancellationToken cancellationToken);

    Task<DemandForecastResult> ForecastDemandAsync(Guid producerId, DemandForecastRequest request, CancellationToken cancellationToken);

    Task<ProductionPlanResult> PlanProductionAsync(Guid producerId, ProductionPlannerRequest request, CancellationToken cancellationToken);

    Task<MaterialForecastResult> ForecastMaterialsAsync(MaterialForecastRequest request, CancellationToken cancellationToken);

    Task<SeasonalPredictionResult> PredictSeasonalTrendAsync(Guid producerId, SeasonalPredictionRequest request, CancellationToken cancellationToken);

    Task<SalesInsightsResult> GenerateSalesInsightsAsync(Guid producerId, SalesInsightsRequest request, CancellationToken cancellationToken);
}

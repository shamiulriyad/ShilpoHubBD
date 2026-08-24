using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAITourismService
{
    Task<TourPlanResult> PlanTourAsync(TourPlanRequest request, CancellationToken cancellationToken);

    Task<BudgetPlanResult> PlanBudgetAsync(BudgetPlanRequest request, CancellationToken cancellationToken);

    Task<RouteOptimizationResult> OptimizeRouteAsync(RouteOptimizationRequest request, CancellationToken cancellationToken);

    Task<TourismTranslationResult> TranslateAsync(TourismTranslationRequest request, CancellationToken cancellationToken);

    Task<CulturalRecommendationResult> RecommendAsync(CulturalRecommendationRequest request, CancellationToken cancellationToken);
}

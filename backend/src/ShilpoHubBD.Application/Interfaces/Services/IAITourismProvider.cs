using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Abstraction over the "intelligence" behind the Tourist AI features. Every method is a pure
// function of pre-fetched context -> result, so a future Gemini/OpenAI/custom-ML implementation
// can be registered in place of DummyAITourismProvider without touching AITourismService or
// AITourismController.
public interface IAITourismProvider
{
    Task<TourPlanResult> PlanTourAsync(TourPlanContext context, CancellationToken cancellationToken);

    Task<BudgetPlanResult> PlanBudgetAsync(BudgetPlanContext context, CancellationToken cancellationToken);

    Task<RouteOptimizationResult> OptimizeRouteAsync(RouteOptimizationContext context, CancellationToken cancellationToken);

    Task<TourismTranslationResult> TranslateAsync(TourismTranslationRequest request, CancellationToken cancellationToken);

    Task<CulturalRecommendationResult> RecommendAsync(CulturalRecommendationContext context, CancellationToken cancellationToken);
}

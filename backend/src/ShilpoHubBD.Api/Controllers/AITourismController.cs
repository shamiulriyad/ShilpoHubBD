using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/ai-tourism")]
public class AITourismController : ControllerBase
{
    private readonly IAITourismService _aiTourismService;

    public AITourismController(IAITourismService aiTourismService)
    {
        _aiTourismService = aiTourismService;
    }

    [HttpPost("tour-plan")]
    public async Task<ActionResult<TourPlanResult>> PlanTour(TourPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiTourismService.PlanTourAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("budget-plan")]
    public async Task<ActionResult<BudgetPlanResult>> PlanBudget(BudgetPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiTourismService.PlanBudgetAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("route-optimization")]
    public async Task<ActionResult<RouteOptimizationResult>> OptimizeRoute(RouteOptimizationRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiTourismService.OptimizeRouteAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("translate")]
    public async Task<ActionResult<TourismTranslationResult>> Translate(TourismTranslationRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiTourismService.TranslateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("cultural-recommendations")]
    public async Task<ActionResult<CulturalRecommendationResult>> GetCulturalRecommendations(
        CulturalRecommendationRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiTourismService.RecommendAsync(request, cancellationToken);
        return Ok(result);
    }
}

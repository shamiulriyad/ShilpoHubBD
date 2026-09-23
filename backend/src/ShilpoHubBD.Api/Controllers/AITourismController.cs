using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/ai-tourism")]
[Authorize]
public class AITourismController : ControllerBase
{
    private readonly IAITourismService _aiTourismService;
    private readonly ISavedTourPlanService _savedPlanService;
    private readonly ILogger<AITourismController> _logger;

    public AITourismController(
        IAITourismService aiTourismService, ISavedTourPlanService savedPlanService, ILogger<AITourismController> logger)
    {
        _aiTourismService = aiTourismService;
        _savedPlanService = savedPlanService;
        _logger = logger;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("tour-plan")]
    public async Task<ActionResult<TourPlanResult>> PlanTour(TourPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiTourismService.PlanTourAsync(request, cancellationToken);

        // Every generated plan goes into the user's trip history. A failure to save must never
        // lose the plan the user just waited for, so it is logged and the plan is still returned.
        try
        {
            result.SavedPlanId = await _savedPlanService.SaveAsync(CurrentUserId, request, result, cancellationToken);
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogError(exc, "Could not save the generated tour plan to the user's history.");
        }

        return Ok(result);
    }

    [HttpGet("saved-plans")]
    public async Task<ActionResult<PagedResult<SavedTourPlanSummaryDto>>> GetMySavedPlans(
        int page = 1, int pageSize = 12, CancellationToken cancellationToken = default)
        => Ok(await _savedPlanService.GetMyPlansAsync(CurrentUserId, page, pageSize, cancellationToken));

    [HttpGet("saved-plans/{id:guid}")]
    public async Task<ActionResult<SavedTourPlanDto>> GetMySavedPlan(Guid id, CancellationToken cancellationToken)
        => Ok(await _savedPlanService.GetMyPlanAsync(CurrentUserId, id, cancellationToken));

    [HttpDelete("saved-plans/{id:guid}")]
    public async Task<IActionResult> DeleteMySavedPlan(Guid id, CancellationToken cancellationToken)
    {
        await _savedPlanService.DeleteMyPlanAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
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

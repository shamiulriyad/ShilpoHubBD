using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.AIBusiness;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/producer/ai-business")]
[Authorize(Roles = RoleNames.Producer)]
public class AIBusinessAssistantController : ControllerBase
{
    private readonly IAIBusinessService _aiBusinessService;

    public AIBusinessAssistantController(IAIBusinessService aiBusinessService)
    {
        _aiBusinessService = aiBusinessService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("price-suggestion")]
    public async Task<ActionResult<PriceSuggestionResult>> SuggestPrice(PriceSuggestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiBusinessService.SuggestPriceAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("description")]
    public async Task<ActionResult<ProductDescriptionResult>> GenerateDescription(ProductDescriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiBusinessService.GenerateDescriptionAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("translate")]
    public async Task<ActionResult<BusinessTranslationResult>> Translate(BusinessTranslationRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiBusinessService.TranslateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("demand-forecast")]
    public async Task<ActionResult<DemandForecastResult>> ForecastDemand(DemandForecastRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiBusinessService.ForecastDemandAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("production-plan")]
    public async Task<ActionResult<ProductionPlanResult>> PlanProduction(ProductionPlannerRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiBusinessService.PlanProductionAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("material-forecast")]
    public async Task<ActionResult<MaterialForecastResult>> ForecastMaterials(MaterialForecastRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiBusinessService.ForecastMaterialsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("seasonal-prediction")]
    public async Task<ActionResult<SeasonalPredictionResult>> PredictSeasonalTrend(SeasonalPredictionRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiBusinessService.PredictSeasonalTrendAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("sales-insights")]
    public async Task<ActionResult<SalesInsightsResult>> GenerateSalesInsights(SalesInsightsRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiBusinessService.GenerateSalesInsightsAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }
}

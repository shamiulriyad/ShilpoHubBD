using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.AIIntelligence;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/ai-intelligence")]
[Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
public class AIIntelligenceController : ControllerBase
{
    private readonly IBusinessPartnerAIService _businessPartnerAIService;

    public AIIntelligenceController(IBusinessPartnerAIService businessPartnerAIService)
    {
        _businessPartnerAIService = businessPartnerAIService;
    }

    [HttpPost("supplier-ranking")]
    public async Task<ActionResult<SupplierRankingResult>> RankSuppliers(SupplierRankingRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPartnerAIService.RankSuppliersAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("quality-prediction")]
    public async Task<ActionResult<QualityPredictionResult>> PredictQuality(QualityPredictionRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPartnerAIService.PredictQualityAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("price-forecast")]
    public async Task<ActionResult<PriceForecastResult>> ForecastPrice(PriceForecastRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPartnerAIService.ForecastPriceAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("delivery-prediction")]
    public async Task<ActionResult<DeliveryPredictionResult>> PredictDelivery(DeliveryPredictionRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPartnerAIService.PredictDeliveryAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("risk-assessment")]
    public async Task<ActionResult<RiskAssessmentResult>> AssessRisk(RiskAssessmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPartnerAIService.AssessRiskAsync(request, cancellationToken);
        return Ok(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ProductIntelligence;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>
/// Business Partner product intelligence: real, already-aggregated database data first (fast); AI
/// insights are a separate, slower call computed only from that same aggregated data.
/// </summary>
[ApiController]
[Route("api/product-intelligence")]
[Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
public class ProductIntelligenceController : ControllerBase
{
    private readonly IProductIntelligenceService _service;

    public ProductIntelligenceController(IProductIntelligenceService service)
    {
        _service = service;
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<ProductIntelligenceDto>> GetIntelligence(
        Guid productId, [FromQuery] ProductIntelligenceRange range, CancellationToken cancellationToken)
        => Ok(await _service.GetIntelligenceAsync(productId, range, cancellationToken));

    [HttpPost("{productId:guid}/ai-insights")]
    public async Task<ActionResult<ProductIntelligenceAiInsightsDto>> GetAiInsights(
        Guid productId, [FromQuery] ProductIntelligenceRange range, CancellationToken cancellationToken)
        => Ok(await _service.GetAiInsightsAsync(productId, range, cancellationToken));
}

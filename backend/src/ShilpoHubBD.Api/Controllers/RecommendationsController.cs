using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    private Guid? CurrentUserIdOrNull
    {
        get
        {
            var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductListItemDto>>> GetForMe(
        CancellationToken cancellationToken, [FromQuery] int count = 8)
    {
        var result = await _recommendationService.GetRecommendedForMeAsync(CurrentUserIdOrNull, count, cancellationToken);
        return Ok(result);
    }

    [HttpGet("similar/{productId:guid}")]
    public async Task<ActionResult<List<ProductListItemDto>>> GetSimilar(
        Guid productId, CancellationToken cancellationToken, [FromQuery] int count = 8)
    {
        var result = await _recommendationService.GetSimilarAsync(productId, count, cancellationToken);
        return Ok(result);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
[Route("api/governance/forecasts")]
public class GovForecastsController : ControllerBase
{
    private readonly IGovForecastService _forecastService;

    public GovForecastsController(IGovForecastService forecastService)
    {
        _forecastService = forecastService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<GovForecastListItemDto>>> GetPaged(
        [FromQuery] GovForecastQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _forecastService.GetForecastsAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GovForecastDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _forecastService.GetForecastByIdAsync(id, cancellationToken));

    /// <summary>Project national heritage-economy metrics forward from snapshot history.</summary>
    [HttpPost("generate")]
    public async Task<ActionResult<GovForecastDto>> Generate(
        GenerateGovForecastRequest request, CancellationToken cancellationToken)
    {
        var result = await _forecastService.GenerateAsync(CurrentUserId, request, cancellationToken);
        return result.Id == Guid.Empty
            ? Ok(result)
            : CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _forecastService.DeleteForecastAsync(id, cancellationToken);
        return NoContent();
    }
}

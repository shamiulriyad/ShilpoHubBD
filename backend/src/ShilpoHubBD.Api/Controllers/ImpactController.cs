using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Impact;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/impact")]
[Authorize]
public class ImpactController : ControllerBase
{
    private readonly IImpactService _impactService;

    public ImpactController(IImpactService impactService)
    {
        _impactService = impactService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mine")]
    public async Task<ActionResult<ImpactSummaryDto>> GetMyImpact(CancellationToken cancellationToken)
    {
        var result = await _impactService.GetMyImpactAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}

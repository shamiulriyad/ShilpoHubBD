using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Community;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/follows/producers")]
[Authorize]
public class ProducerFollowsController : ControllerBase
{
    private readonly IProducerFollowService _followService;

    public ProducerFollowsController(IProducerFollowService followService)
    {
        _followService = followService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<FollowedProducerDto>>> GetMyFollowedProducers(CancellationToken cancellationToken)
    {
        var result = await _followService.GetMyFollowedProducersAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{producerId:guid}")]
    public async Task<IActionResult> Follow(Guid producerId, CancellationToken cancellationToken)
    {
        await _followService.FollowAsync(CurrentUserId, producerId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{producerId:guid}")]
    public async Task<IActionResult> Unfollow(Guid producerId, CancellationToken cancellationToken)
    {
        await _followService.UnfollowAsync(CurrentUserId, producerId, cancellationToken);
        return NoContent();
    }
}

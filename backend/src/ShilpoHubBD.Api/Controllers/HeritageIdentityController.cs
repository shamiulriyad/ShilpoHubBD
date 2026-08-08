using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/heritage-identity")]
public class HeritageIdentityController : ControllerBase
{
    private readonly IHeritageIdentityService _heritageIdentityService;

    public HeritageIdentityController(IHeritageIdentityService heritageIdentityService)
    {
        _heritageIdentityService = heritageIdentityService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("verified")]
    public async Task<ActionResult<PagedResult<ProducerHeritageIdentityDto>>> GetVerified(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _heritageIdentityService.GetVerifiedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{producerId:guid}")]
    public async Task<ActionResult<ProducerHeritageIdentityDto>> GetByProducer(Guid producerId, CancellationToken cancellationToken)
    {
        var result = await _heritageIdentityService.GetByProducerIdAsync(producerId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{producerId:guid}")]
    public async Task<ActionResult<ProducerHeritageIdentityDto>> Upsert(
        Guid producerId, UpsertHeritageIdentityRequest request, CancellationToken cancellationToken)
    {
        var result = await _heritageIdentityService.UpsertAsync(producerId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{producerId:guid}/verify")]
    public async Task<ActionResult<ProducerHeritageIdentityDto>> Verify(
        Guid producerId, VerifyHeritageIdentityRequest request, CancellationToken cancellationToken)
    {
        var result = await _heritageIdentityService.VerifyAsync(producerId, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpDelete("{producerId:guid}")]
    public async Task<IActionResult> Delete(Guid producerId, CancellationToken cancellationToken)
    {
        await _heritageIdentityService.DeleteAsync(producerId, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }

    [HttpGet("{producerId:guid}/score")]
    public async Task<ActionResult<LegacyScoreDto>> GetScore(Guid producerId, CancellationToken cancellationToken)
    {
        var result = await _heritageIdentityService.GetScoreAsync(producerId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{producerId:guid}/score/history")]
    public async Task<ActionResult<PagedResult<LegacyScoreHistoryEntryDto>>> GetScoreHistory(
        Guid producerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _heritageIdentityService.GetScoreHistoryAsync(producerId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{producerId:guid}/score/recalculate")]
    public async Task<ActionResult<LegacyScoreDto>> RecalculateScore(Guid producerId, CancellationToken cancellationToken)
    {
        var result = await _heritageIdentityService.RecalculateScoreAsync(producerId, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}

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
[Route("api/governance/heritage-intelligence")]
public class HeritageIntelligenceController : ControllerBase
{
    private readonly IHeritageIntelligenceService _intelligenceService;

    public HeritageIntelligenceController(IHeritageIntelligenceService intelligenceService)
    {
        _intelligenceService = intelligenceService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Compute a heritage-intelligence index for a scope/period; persisted unless Persist=false.</summary>
    [HttpPost("compute")]
    public async Task<ActionResult<HeritageIndexRecordDto>> Compute(
        ComputeHeritageIndexRequest request, CancellationToken cancellationToken)
    {
        var result = await _intelligenceService.ComputeAsync(CurrentUserId, request, cancellationToken);
        return result.Id == Guid.Empty
            ? Ok(result)
            : CreatedAtAction(nameof(GetRecord), new { id = result.Id }, result);
    }

    [HttpGet("records")]
    public async Task<ActionResult<PagedResult<HeritageIndexRecordListItemDto>>> GetRecords(
        [FromQuery] HeritageIndexQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _intelligenceService.GetRecordsAsync(query, cancellationToken));

    [HttpGet("records/{id:guid}")]
    public async Task<ActionResult<HeritageIndexRecordDto>> GetRecord(Guid id, CancellationToken cancellationToken)
        => Ok(await _intelligenceService.GetRecordByIdAsync(id, cancellationToken));

    [HttpDelete("records/{id:guid}")]
    public async Task<IActionResult> DeleteRecord(Guid id, CancellationToken cancellationToken)
    {
        await _intelligenceService.DeleteRecordAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Score history for one index + scope across saved records, oldest to newest.</summary>
    [HttpGet("trends")]
    public async Task<ActionResult<HeritageIndexTrendDto>> GetTrend(
        [FromQuery] string indexType,
        [FromQuery] string scope,
        [FromQuery] Guid? scopeId,
        [FromQuery] string? craftLabel,
        [FromQuery] int take = 12,
        CancellationToken cancellationToken = default)
        => Ok(await _intelligenceService.GetTrendAsync(
            indexType, scope, scopeId, craftLabel, take, cancellationToken));
}

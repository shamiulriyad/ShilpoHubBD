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
[Route("api/governance/dashboard")]
public class NationalDashboardController : ControllerBase
{
    private readonly INationalDashboardService _dashboardService;

    public NationalDashboardController(INationalDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Live national heritage-economy overview, optionally scoped to a date window.</summary>
    [HttpGet("overview")]
    public async Task<ActionResult<NationalDashboardOverviewDto>> GetOverview(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetOverviewAsync(from, to, cancellationToken));

    /// <summary>District rankings by sales, producers, products, villages or orders.</summary>
    [HttpGet("district-rankings")]
    public async Task<ActionResult<List<DistrictRankingDto>>> GetDistrictRankings(
        [FromQuery] string? metric,
        [FromQuery] int top = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
        => Ok(await _dashboardService.GetDistrictRankingsAsync(metric, top, from, to, cancellationToken));

    [HttpGet("snapshots")]
    public async Task<ActionResult<PagedResult<NationalDashboardSnapshotListItemDto>>> GetSnapshots(
        [FromQuery] NationalDashboardSnapshotQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetSnapshotsAsync(query, cancellationToken));

    [HttpGet("snapshots/{id:guid}")]
    public async Task<ActionResult<NationalDashboardSnapshotDto>> GetSnapshot(
        Guid id, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetSnapshotByIdAsync(id, cancellationToken));

    /// <summary>Compute and persist a snapshot of the given period's metrics.</summary>
    [HttpPost("snapshots")]
    public async Task<ActionResult<NationalDashboardSnapshotDto>> CaptureSnapshot(
        CreateNationalDashboardSnapshotRequest request, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.CaptureSnapshotAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetSnapshot), new { id = result.Id }, result);
    }

    [HttpDelete("snapshots/{id:guid}")]
    public async Task<IActionResult> DeleteSnapshot(Guid id, CancellationToken cancellationToken)
    {
        await _dashboardService.DeleteSnapshotAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Metric values across captured snapshots, oldest to newest, for trend charts.</summary>
    [HttpGet("trends")]
    public async Task<ActionResult<DashboardTrendDto>> GetTrend(
        [FromQuery] string metric,
        [FromQuery] string? period,
        [FromQuery] int take = 12,
        CancellationToken cancellationToken = default)
        => Ok(await _dashboardService.GetTrendAsync(metric, period, take, cancellationToken));
}

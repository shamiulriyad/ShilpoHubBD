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
[Route("api/governance/monitoring")]
public class MonitoringController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;

    public MonitoringController(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Run the rule-based fraud / fake-product / review-abuse / QR-anomaly scans.</summary>
    [HttpPost("scans")]
    public async Task<ActionResult<MonitoringScanResultDto>> RunScan(
        RunMonitoringScanRequest request, CancellationToken cancellationToken)
        => Ok(await _monitoringService.RunScanAsync(CurrentUserId, request, cancellationToken));

    [HttpGet("flags")]
    public async Task<ActionResult<PagedResult<MonitoringFlagListItemDto>>> GetFlags(
        [FromQuery] MonitoringFlagQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _monitoringService.GetFlagsAsync(query, cancellationToken));

    [HttpGet("flags/{id:guid}")]
    public async Task<ActionResult<MonitoringFlagDto>> GetFlag(Guid id, CancellationToken cancellationToken)
        => Ok(await _monitoringService.GetFlagByIdAsync(id, cancellationToken));

    [HttpPost("flags")]
    public async Task<ActionResult<MonitoringFlagDto>> CreateFlag(
        CreateMonitoringFlagRequest request, CancellationToken cancellationToken)
    {
        var result = await _monitoringService.CreateFlagAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetFlag), new { id = result.Id }, result);
    }

    [HttpPost("flags/{id:guid}/status")]
    public async Task<ActionResult<MonitoringFlagDto>> UpdateFlagStatus(
        Guid id, UpdateMonitoringFlagStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _monitoringService.UpdateFlagStatusAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("flags/{id:guid}/assign")]
    public async Task<ActionResult<MonitoringFlagDto>> AssignFlag(
        Guid id, AssignMonitoringFlagRequest request, CancellationToken cancellationToken)
        => Ok(await _monitoringService.AssignFlagAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("flags/{id:guid}/notes")]
    public async Task<ActionResult<MonitoringFlagDto>> AddFlagNote(
        Guid id, AddMonitoringFlagNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _monitoringService.AddFlagNoteAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("flags/{id:guid}")]
    public async Task<IActionResult> DeleteFlag(Guid id, CancellationToken cancellationToken)
    {
        await _monitoringService.DeleteFlagAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>QR-verification volume, invalid-scan rate and anomalous products.</summary>
    [HttpGet("qr/overview")]
    public async Task<ActionResult<QrMonitoringOverviewDto>> GetQrOverview(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
        => Ok(await _monitoringService.GetQrOverviewAsync(from, to, cancellationToken));
}

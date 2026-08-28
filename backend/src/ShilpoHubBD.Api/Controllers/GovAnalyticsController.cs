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
[Route("api/governance/analytics")]
public class GovAnalyticsController : ControllerBase
{
    private readonly IGovReportService _reportService;

    public GovAnalyticsController(IGovReportService reportService)
    {
        _reportService = reportService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>District-keyed values for a chosen metric, to join to a client-side boundary file.</summary>
    [HttpGet("gis/districts")]
    public async Task<ActionResult<GisMapDto>> GetGisDistricts(
        [FromQuery] GisMapQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _reportService.GetGisMapAsync(query, cancellationToken));

    [HttpGet("exports")]
    public async Task<ActionResult<PagedResult<AnalyticsExportDto>>> GetExports(
        [FromQuery] AnalyticsExportQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _reportService.GetExportsAsync(CurrentUserId, query, cancellationToken));

    [HttpGet("exports/{id:guid}")]
    public async Task<ActionResult<AnalyticsExportDto>> GetExport(Guid id, CancellationToken cancellationToken)
        => Ok(await _reportService.GetExportByIdAsync(id, cancellationToken));

    /// <summary>Request a downloadable export of a governance dataset (metadata only; file produced offline).</summary>
    [HttpPost("exports")]
    public async Task<ActionResult<AnalyticsExportDto>> RequestExport(
        CreateAnalyticsExportRequest request, CancellationToken cancellationToken)
    {
        var result = await _reportService.RequestExportAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetExport), new { id = result.Id }, result);
    }

    /// <summary>Mark an export Completed (with file metadata) or Failed — called by the export worker.</summary>
    [HttpPost("exports/{id:guid}/complete")]
    public async Task<ActionResult<AnalyticsExportDto>> CompleteExport(
        Guid id, CompleteAnalyticsExportRequest request, CancellationToken cancellationToken)
        => Ok(await _reportService.CompleteExportAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("exports/{id:guid}")]
    public async Task<IActionResult> DeleteExport(Guid id, CancellationToken cancellationToken)
    {
        await _reportService.DeleteExportAsync(id, cancellationToken);
        return NoContent();
    }
}

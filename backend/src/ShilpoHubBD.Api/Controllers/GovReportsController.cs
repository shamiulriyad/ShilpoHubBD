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
[Route("api/governance/reports")]
public class GovReportsController : ControllerBase
{
    private readonly IGovReportService _reportService;

    public GovReportsController(IGovReportService reportService)
    {
        _reportService = reportService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<GovReportListItemDto>>> GetPaged(
        [FromQuery] GovReportQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _reportService.GetReportsAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GovReportDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _reportService.GetReportByIdAsync(id, cancellationToken));

    /// <summary>Generate a period report assembling data from the dashboard, monitoring and funding modules.</summary>
    [HttpPost("generate")]
    public async Task<ActionResult<GovReportDto>> Generate(
        GenerateGovReportRequest request, CancellationToken cancellationToken)
    {
        var result = await _reportService.GenerateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GovReportDto>> Update(
        Guid id, UpdateGovReportRequest request, CancellationToken cancellationToken)
        => Ok(await _reportService.UpdateReportAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _reportService.DeleteReportAsync(id, cancellationToken);
        return NoContent();
    }
}

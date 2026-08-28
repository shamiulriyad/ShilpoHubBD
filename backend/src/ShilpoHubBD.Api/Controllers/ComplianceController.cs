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
[Route("api/governance/compliance")]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceService _complianceService;

    public ComplianceController(IComplianceService complianceService)
    {
        _complianceService = complianceService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("records")]
    public async Task<ActionResult<PagedResult<ComplianceRecordListItemDto>>> GetPaged(
        [FromQuery] ComplianceQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _complianceService.GetPagedAsync(query, cancellationToken));

    [HttpGet("records/{id:guid}")]
    public async Task<ActionResult<ComplianceRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _complianceService.GetByIdAsync(id, cancellationToken));

    [HttpPost("records")]
    public async Task<ActionResult<ComplianceRecordDto>> Create(
        CreateComplianceRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _complianceService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("records/{id:guid}")]
    public async Task<ActionResult<ComplianceRecordDto>> Update(
        Guid id, UpdateComplianceRecordRequest request, CancellationToken cancellationToken)
        => Ok(await _complianceService.UpdateAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("records/{id:guid}/requirements")]
    public async Task<ActionResult<ComplianceRecordDto>> UpsertRequirement(
        Guid id, UpsertComplianceRequirementRequest request, CancellationToken cancellationToken)
        => Ok(await _complianceService.UpsertRequirementAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("records/{id:guid}/requirements/{requirementId:guid}")]
    public async Task<ActionResult<ComplianceRecordDto>> RemoveRequirement(
        Guid id, Guid requirementId, CancellationToken cancellationToken)
        => Ok(await _complianceService.RemoveRequirementAsync(CurrentUserId, id, requirementId, cancellationToken));

    [HttpDelete("records/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _complianceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

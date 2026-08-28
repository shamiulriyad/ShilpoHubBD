using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.HeritageInnovationHub},{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
[Route("api/heritage-database/risk")]
public class HeritageRiskController : ControllerBase
{
    private readonly IHeritageRiskService _riskService;

    public HeritageRiskController(IHeritageRiskService riskService)
    {
        _riskService = riskService;
    }

    private const string StewardRoles = $"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}";

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<HeritageRiskRecordDto>>> GetPaged(
        [FromQuery] HeritageRiskQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _riskService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HeritageRiskRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _riskService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = StewardRoles)]
    [HttpPost]
    public async Task<ActionResult<HeritageRiskRecordDto>> Create(
        CreateHeritageRiskRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _riskService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = StewardRoles)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HeritageRiskRecordDto>> Update(
        Guid id, UpdateHeritageRiskRecordRequest request, CancellationToken cancellationToken)
        => Ok(await _riskService.UpdateAsync(CurrentUserId, id, request, cancellationToken));

    [Authorize(Roles = StewardRoles)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _riskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

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
[Route("api/governance/funding/programs")]
public class FundingProgramsController : ControllerBase
{
    private readonly IFundingService _fundingService;

    public FundingProgramsController(IFundingService fundingService)
    {
        _fundingService = fundingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<FundingProgramListItemDto>>> GetPaged(
        [FromQuery] FundingProgramQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _fundingService.GetProgramsAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FundingProgramDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _fundingService.GetProgramByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<FundingProgramDto>> Create(
        CreateFundingProgramRequest request, CancellationToken cancellationToken)
    {
        var result = await _fundingService.CreateProgramAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FundingProgramDto>> Update(
        Guid id, UpdateFundingProgramRequest request, CancellationToken cancellationToken)
        => Ok(await _fundingService.UpdateProgramAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _fundingService.DeleteProgramAsync(id, cancellationToken);
        return NoContent();
    }
}

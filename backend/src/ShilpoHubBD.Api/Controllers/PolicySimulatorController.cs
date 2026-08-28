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
[Route("api/governance/policy-simulator")]
public class PolicySimulatorController : ControllerBase
{
    private readonly IPolicySimulationService _simulationService;

    public PolicySimulatorController(IPolicySimulationService simulationService)
    {
        _simulationService = simulationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Run a policy scenario against a live baseline; persisted unless Persist=false.</summary>
    [HttpPost("simulations")]
    public async Task<ActionResult<PolicySimulationDto>> Run(
        RunPolicySimulationRequest request, CancellationToken cancellationToken)
    {
        var result = await _simulationService.RunAsync(CurrentUserId, request, cancellationToken);
        return result.Id == Guid.Empty
            ? Ok(result)
            : CreatedAtAction(nameof(GetSimulation), new { id = result.Id }, result);
    }

    [HttpGet("simulations")]
    public async Task<ActionResult<PagedResult<PolicySimulationListItemDto>>> GetSimulations(
        [FromQuery] PolicySimulationQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _simulationService.GetSimulationsAsync(query, cancellationToken));

    [HttpGet("simulations/{id:guid}")]
    public async Task<ActionResult<PolicySimulationDto>> GetSimulation(Guid id, CancellationToken cancellationToken)
        => Ok(await _simulationService.GetSimulationByIdAsync(id, cancellationToken));

    [HttpDelete("simulations/{id:guid}")]
    public async Task<IActionResult> DeleteSimulation(Guid id, CancellationToken cancellationToken)
    {
        await _simulationService.DeleteSimulationAsync(id, cancellationToken);
        return NoContent();
    }
}

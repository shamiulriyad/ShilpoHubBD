using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/innovation-lab/preservation-strategies")]
public class PreservationStrategiesController : ControllerBase
{
    private readonly IPreservationStrategyService _strategyService;

    public PreservationStrategiesController(IPreservationStrategyService strategyService)
    {
        _strategyService = strategyService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsResearcher =>
        User.IsInRole(RoleNames.HeritageInnovationHub) || User.IsInRole(RoleNames.GovernmentNGO)
        || User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<PreservationStrategyListItemDto>>> GetMine(
        [FromQuery] PreservationStrategyQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _strategyService.GetMineAsync(CurrentUserId, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PreservationStrategyDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _strategyService.GetByIdAsync(CurrentUserId, id, cancellationToken));

    [Authorize(Roles = InnovationExperimentsController.ResearcherRoles)]
    [HttpPost]
    public async Task<ActionResult<PreservationStrategyDetailDto>> Create(
        CreatePreservationStrategyRequest request, CancellationToken cancellationToken)
    {
        var result = await _strategyService.CreateAsync(CurrentUserId, IsResearcher, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PreservationStrategyDetailDto>> Update(
        Guid id, UpdatePreservationStrategyRequest request, CancellationToken cancellationToken)
        => Ok(await _strategyService.UpdateAsync(CurrentUserId, IsResearcher, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _strategyService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/objectives")]
    public async Task<ActionResult<StrategyObjectiveDto>> AddObjective(
        Guid id, CreateStrategyObjectiveRequest request, CancellationToken cancellationToken)
        => Ok(await _strategyService.AddObjectiveAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/objectives/{objectiveId:guid}")]
    public async Task<ActionResult<StrategyObjectiveDto>> UpdateObjective(
        Guid id, Guid objectiveId, UpdateStrategyObjectiveRequest request, CancellationToken cancellationToken)
        => Ok(await _strategyService.UpdateObjectiveAsync(CurrentUserId, id, objectiveId, request, cancellationToken));

    [HttpDelete("{id:guid}/objectives/{objectiveId:guid}")]
    public async Task<IActionResult> DeleteObjective(Guid id, Guid objectiveId, CancellationToken cancellationToken)
    {
        await _strategyService.DeleteObjectiveAsync(CurrentUserId, id, objectiveId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/actions")]
    public async Task<ActionResult<StrategyActionDto>> AddAction(
        Guid id, CreateStrategyActionRequest request, CancellationToken cancellationToken)
        => Ok(await _strategyService.AddActionAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/actions/{actionId:guid}")]
    public async Task<ActionResult<StrategyActionDto>> UpdateAction(
        Guid id, Guid actionId, UpdateStrategyActionRequest request, CancellationToken cancellationToken)
        => Ok(await _strategyService.UpdateActionAsync(CurrentUserId, id, actionId, request, cancellationToken));

    [HttpDelete("{id:guid}/actions/{actionId:guid}")]
    public async Task<IActionResult> DeleteAction(Guid id, Guid actionId, CancellationToken cancellationToken)
    {
        await _strategyService.DeleteActionAsync(CurrentUserId, id, actionId, cancellationToken);
        return NoContent();
    }
}

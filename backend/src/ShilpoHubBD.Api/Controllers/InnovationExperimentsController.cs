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
[Route("api/innovation-lab/experiments")]
public class InnovationExperimentsController : ControllerBase
{
    private readonly IInnovationExperimentService _experimentService;

    public InnovationExperimentsController(IInnovationExperimentService experimentService)
    {
        _experimentService = experimentService;
    }

    internal const string ResearcherRoles =
        $"{RoleNames.HeritageInnovationHub},{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}";

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsResearcher =>
        User.IsInRole(RoleNames.HeritageInnovationHub) || User.IsInRole(RoleNames.GovernmentNGO)
        || User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<InnovationExperimentListItemDto>>> GetMine(
        [FromQuery] InnovationExperimentQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _experimentService.GetMineAsync(CurrentUserId, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InnovationExperimentDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _experimentService.GetByIdAsync(CurrentUserId, id, cancellationToken));

    [Authorize(Roles = ResearcherRoles)]
    [HttpPost]
    public async Task<ActionResult<InnovationExperimentDetailDto>> Create(
        CreateInnovationExperimentRequest request, CancellationToken cancellationToken)
    {
        var result = await _experimentService.CreateAsync(CurrentUserId, IsResearcher, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InnovationExperimentDetailDto>> Update(
        Guid id, UpdateInnovationExperimentRequest request, CancellationToken cancellationToken)
        => Ok(await _experimentService.UpdateAsync(CurrentUserId, IsResearcher, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _experimentService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/versions")]
    public async Task<ActionResult<ExperimentVersionDto>> AddVersion(
        Guid id, CreateExperimentVersionRequest request, CancellationToken cancellationToken)
        => Ok(await _experimentService.AddVersionAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/runs")]
    public async Task<ActionResult<TrainingRunDto>> CreateRun(
        Guid id, CreateTrainingRunRequest request, CancellationToken cancellationToken)
        => Ok(await _experimentService.CreateRunAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/runs/{runId:guid}")]
    public async Task<ActionResult<TrainingRunDto>> UpdateRun(
        Guid id, Guid runId, UpdateTrainingRunRequest request, CancellationToken cancellationToken)
        => Ok(await _experimentService.UpdateRunAsync(CurrentUserId, id, runId, request, cancellationToken));

    [HttpDelete("{id:guid}/runs/{runId:guid}")]
    public async Task<IActionResult> DeleteRun(Guid id, Guid runId, CancellationToken cancellationToken)
    {
        await _experimentService.DeleteRunAsync(CurrentUserId, id, runId, cancellationToken);
        return NoContent();
    }
}

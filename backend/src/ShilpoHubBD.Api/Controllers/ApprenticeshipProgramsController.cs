using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/apprenticeship-programs")]
public class ApprenticeshipProgramsController : ControllerBase
{
    private readonly IApprenticeshipProgramService _programService;

    public ApprenticeshipProgramsController(IApprenticeshipProgramService programService)
    {
        _programService = programService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private Guid? CurrentUserIdOrNull => User.Identity?.IsAuthenticated == true ? CurrentUserId : null;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ApprenticeshipProgramListItemDto>>> GetPublished(
        [FromQuery] ApprenticeshipProgramQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _programService.GetPublishedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApprenticeshipProgramDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _programService.GetByIdAsync(id, CurrentUserIdOrNull, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<ApprenticeshipProgramListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _programService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApprenticeshipProgramDto>> Create(CreateApprenticeshipProgramRequest request, CancellationToken cancellationToken)
    {
        var result = await _programService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApprenticeshipProgramDto>> Update(Guid id, UpdateApprenticeshipProgramRequest request, CancellationToken cancellationToken)
    {
        var result = await _programService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<ApprenticeshipProgramDto>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _programService.PublishAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<ApprenticeshipProgramDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _programService.CloseAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _programService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/milestones")]
    public async Task<ActionResult<TrainingMilestoneDto>> AddMilestone(Guid id, CreateTrainingMilestoneRequest request, CancellationToken cancellationToken)
    {
        var result = await _programService.AddMilestoneAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id:guid}/milestones/{milestoneId:guid}")]
    public async Task<ActionResult<TrainingMilestoneDto>> UpdateMilestone(
        Guid id, Guid milestoneId, UpdateTrainingMilestoneRequest request, CancellationToken cancellationToken)
    {
        var result = await _programService.UpdateMilestoneAsync(CurrentUserId, id, milestoneId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}/milestones/{milestoneId:guid}")]
    public async Task<IActionResult> DeleteMilestone(Guid id, Guid milestoneId, CancellationToken cancellationToken)
    {
        await _programService.DeleteMilestoneAsync(CurrentUserId, id, milestoneId, cancellationToken);
        return NoContent();
    }
}

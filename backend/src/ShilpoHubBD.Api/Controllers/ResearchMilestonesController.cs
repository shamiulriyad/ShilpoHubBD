using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/research/projects/{projectId:guid}/milestones")]
public class ResearchMilestonesController : ControllerBase
{
    private readonly IResearchMilestoneService _milestoneService;

    public ResearchMilestonesController(IResearchMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ResearchMilestoneDto>>> GetForProject(
        Guid projectId, CancellationToken cancellationToken)
        => Ok(await _milestoneService.GetForProjectAsync(CurrentUserId, projectId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ResearchMilestoneDto>> Create(
        Guid projectId, CreateResearchMilestoneRequest request, CancellationToken cancellationToken)
        => Ok(await _milestoneService.CreateAsync(CurrentUserId, projectId, request, cancellationToken));

    [HttpPut("{milestoneId:guid}")]
    public async Task<ActionResult<ResearchMilestoneDto>> Update(
        Guid projectId, Guid milestoneId, UpdateResearchMilestoneRequest request, CancellationToken cancellationToken)
        => Ok(await _milestoneService.UpdateAsync(CurrentUserId, projectId, milestoneId, request, cancellationToken));

    [HttpDelete("{milestoneId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid milestoneId, CancellationToken cancellationToken)
    {
        await _milestoneService.DeleteAsync(CurrentUserId, projectId, milestoneId, cancellationToken);
        return NoContent();
    }
}

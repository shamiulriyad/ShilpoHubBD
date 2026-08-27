using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/research/projects/{projectId:guid}/tasks")]
public class ResearchTasksController : ControllerBase
{
    private readonly IResearchTaskService _taskService;

    public ResearchTasksController(IResearchTaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ResearchTaskDto>>> GetForProject(
        Guid projectId, [FromQuery] ResearchTaskQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _taskService.GetForProjectAsync(CurrentUserId, projectId, query, cancellationToken));

    [HttpGet("{taskId:guid}")]
    public async Task<ActionResult<ResearchTaskDto>> GetById(
        Guid projectId, Guid taskId, CancellationToken cancellationToken)
        => Ok(await _taskService.GetByIdAsync(CurrentUserId, projectId, taskId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ResearchTaskDto>> Create(
        Guid projectId, CreateResearchTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateAsync(CurrentUserId, projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { projectId, taskId = result.Id }, result);
    }

    [HttpPut("{taskId:guid}")]
    public async Task<ActionResult<ResearchTaskDto>> Update(
        Guid projectId, Guid taskId, UpdateResearchTaskRequest request, CancellationToken cancellationToken)
        => Ok(await _taskService.UpdateAsync(CurrentUserId, projectId, taskId, request, cancellationToken));

    [HttpPut("{taskId:guid}/status")]
    public async Task<ActionResult<ResearchTaskDto>> UpdateStatus(
        Guid projectId, Guid taskId, UpdateResearchTaskStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _taskService.UpdateStatusAsync(CurrentUserId, projectId, taskId, request, cancellationToken));

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(CurrentUserId, projectId, taskId, cancellationToken);
        return NoContent();
    }
}

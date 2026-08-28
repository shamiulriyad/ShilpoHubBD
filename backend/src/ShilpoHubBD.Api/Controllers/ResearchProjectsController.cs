using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/research/projects")]
public class ResearchProjectsController : ControllerBase
{
    private readonly IResearchProjectService _projectService;

    public ResearchProjectsController(IResearchProjectService projectService)
    {
        _projectService = projectService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private const string ProjectCreatorRoles =
        $"{RoleNames.HeritageInnovationHub},{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}";

    [HttpGet]
    public async Task<ActionResult<PagedResult<ResearchProjectListItemDto>>> GetMine(
        [FromQuery] ResearchProjectQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _projectService.GetMineAsync(CurrentUserId, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResearchProjectDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _projectService.GetByIdAsync(CurrentUserId, id, cancellationToken));

    [Authorize(Roles = ProjectCreatorRoles)]
    [HttpPost]
    public async Task<ActionResult<ResearchProjectDetailDto>> Create(
        CreateResearchProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResearchProjectDetailDto>> Update(
        Guid id, UpdateResearchProjectRequest request, CancellationToken cancellationToken)
        => Ok(await _projectService.UpdateAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ResearchProjectDetailDto>> UpdateStatus(
        Guid id, UpdateResearchProjectStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _projectService.UpdateStatusAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _projectService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<List<ResearchProjectMemberDto>>> GetMembers(Guid id, CancellationToken cancellationToken)
        => Ok(await _projectService.GetMembersAsync(CurrentUserId, id, cancellationToken));

    [HttpPost("{id:guid}/members")]
    public async Task<ActionResult<ResearchProjectMemberDto>> AddMember(
        Guid id, AddResearchProjectMemberRequest request, CancellationToken cancellationToken)
        => Ok(await _projectService.AddMemberAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/members/{memberId:guid}/role")]
    public async Task<ActionResult<ResearchProjectMemberDto>> UpdateMemberRole(
        Guid id, Guid memberId, UpdateResearchMemberRoleRequest request, CancellationToken cancellationToken)
        => Ok(await _projectService.UpdateMemberRoleAsync(CurrentUserId, id, memberId, request, cancellationToken));

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId, CancellationToken cancellationToken)
    {
        await _projectService.RemoveMemberAsync(CurrentUserId, id, memberId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/activity")]
    public async Task<ActionResult<List<ResearchActivityDto>>> GetActivity(
        Guid id, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
        => Ok(await _projectService.GetActivityAsync(CurrentUserId, id, take, cancellationToken));
}

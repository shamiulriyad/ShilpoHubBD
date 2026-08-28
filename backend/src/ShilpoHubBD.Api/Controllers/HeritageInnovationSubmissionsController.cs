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
[Route("api/innovation-lab/submissions")]
public class HeritageInnovationSubmissionsController : ControllerBase
{
    private readonly IHeritageInnovationSubmissionService _submissionService;

    public HeritageInnovationSubmissionsController(IHeritageInnovationSubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsResearcher =>
        User.IsInRole(RoleNames.HeritageInnovationHub) || User.IsInRole(RoleNames.GovernmentNGO)
        || User.IsInRole(RoleNames.SuperAdmin);

    private bool IsReviewer => User.IsInRole(RoleNames.GovernmentNGO) || User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<HeritageInnovationSubmissionListItemDto>>> GetAccessible(
        [FromQuery] HeritageInnovationSubmissionQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _submissionService.GetAccessibleAsync(CurrentUserId, IsReviewer, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HeritageInnovationSubmissionDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _submissionService.GetByIdAsync(CurrentUserId, IsReviewer, id, cancellationToken));

    [Authorize(Roles = InnovationExperimentsController.ResearcherRoles)]
    [HttpPost]
    public async Task<ActionResult<HeritageInnovationSubmissionDetailDto>> Create(
        CreateHeritageInnovationSubmissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _submissionService.CreateAsync(CurrentUserId, IsResearcher, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HeritageInnovationSubmissionDetailDto>> Update(
        Guid id, UpdateHeritageInnovationSubmissionRequest request, CancellationToken cancellationToken)
        => Ok(await _submissionService.UpdateAsync(CurrentUserId, IsResearcher, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _submissionService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<HeritageInnovationSubmissionDetailDto>> Submit(Guid id, CancellationToken cancellationToken)
        => Ok(await _submissionService.SubmitAsync(CurrentUserId, id, cancellationToken));

    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<HeritageInnovationSubmissionDetailDto>> Withdraw(Guid id, CancellationToken cancellationToken)
        => Ok(await _submissionService.WithdrawAsync(CurrentUserId, id, cancellationToken));

    [HttpPost("{id:guid}/team-members")]
    public async Task<ActionResult<SubmissionTeamMemberDto>> AddTeamMember(
        Guid id, AddSubmissionTeamMemberRequest request, CancellationToken cancellationToken)
        => Ok(await _submissionService.AddTeamMemberAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("{id:guid}/team-members/{memberId:guid}")]
    public async Task<IActionResult> RemoveTeamMember(Guid id, Guid memberId, CancellationToken cancellationToken)
    {
        await _submissionService.RemoveTeamMemberAsync(CurrentUserId, id, memberId, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = $"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/reviews")]
    public async Task<ActionResult<SubmissionReviewDto>> AddReview(
        Guid id, CreateSubmissionReviewRequest request, CancellationToken cancellationToken)
        => Ok(await _submissionService.AddReviewAsync(CurrentUserId, IsReviewer, id, request, cancellationToken));

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<List<SubmissionEventDto>>> GetHistory(
        Guid id, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
        => Ok(await _submissionService.GetHistoryAsync(CurrentUserId, IsReviewer, id, take, cancellationToken));
}

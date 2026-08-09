using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/design-collaborations")]
[Authorize]
public class DesignCollaborationsController : ControllerBase
{
    private readonly IDesignCollaborationService _designCollaborationService;

    public DesignCollaborationsController(IDesignCollaborationService designCollaborationService)
    {
        _designCollaborationService = designCollaborationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectListItemDto>>> GetMine(
        [FromQuery] ProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.GetForBusinessPartnerAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("received")]
    public async Task<ActionResult<PagedResult<ProjectListItemDto>>> GetReceived(
        [FromQuery] ProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.GetForProducerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/respond")]
    public async Task<ActionResult<ProjectDto>> Respond(Guid id, CollaborationResponseRequest request, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.RespondAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<DesignCommentDto>> AddComment(Guid id, AddCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.AddCommentAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/files")]
    public async Task<ActionResult<DesignFileDto>> AddFile(Guid id, DesignFileInput request, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.AddFileAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/revisions")]
    public async Task<ActionResult<DesignRevisionDto>> SubmitRevision(Guid id, SubmitRevisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.SubmitRevisionAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/revisions/{revisionId:guid}/decision")]
    public async Task<ActionResult<DesignRevisionDto>> DecideRevision(
        Guid id, Guid revisionId, RevisionDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.DecideRevisionAsync(id, revisionId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ProjectDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.CompleteAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ProjectDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _designCollaborationService.CancelAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}

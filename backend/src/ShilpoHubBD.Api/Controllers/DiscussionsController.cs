using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Community;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/discussions")]
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;

    public DiscussionsController(IDiscussionService discussionService)
    {
        _discussionService = discussionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<DiscussionThreadListItemDto>>> GetAll(
        [FromQuery] DiscussionQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _discussionService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DiscussionThreadDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _discussionService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<DiscussionThreadDto>> Create(CreateDiscussionThreadRequest request, CancellationToken cancellationToken)
    {
        var result = await _discussionService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPost("{id:guid}/replies")]
    public async Task<ActionResult<DiscussionThreadDto>> Reply(Guid id, CreateDiscussionReplyRequest request, CancellationToken cancellationToken)
    {
        var result = await _discussionService.ReplyAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteThread(Guid id, CancellationToken cancellationToken)
    {
        await _discussionService.DeleteThreadAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/replies/{replyId:guid}")]
    public async Task<IActionResult> DeleteReply(Guid id, Guid replyId, CancellationToken cancellationToken)
    {
        await _discussionService.DeleteReplyAsync(id, replyId, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}

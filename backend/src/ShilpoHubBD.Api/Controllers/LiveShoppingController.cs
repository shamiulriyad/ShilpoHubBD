using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.LiveShopping;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/live-events")]
public class LiveShoppingController : ControllerBase
{
    private readonly ILiveShoppingService _liveShoppingService;

    public LiveShoppingController(ILiveShoppingService liveShoppingService)
    {
        _liveShoppingService = liveShoppingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<LiveEventListItemDto>>> GetAll(
        [FromQuery] LiveEventQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _liveShoppingService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LiveEventDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveShoppingService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<LiveEventDto>> Create(CreateLiveEventRequest request, CancellationToken cancellationToken)
    {
        var result = await _liveShoppingService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<LiveEventDto>> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveShoppingService.StartAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/end")]
    public async Task<ActionResult<LiveEventDto>> End(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveShoppingService.EndAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<LiveEventCommentDto>> AddComment(Guid id, AddLiveCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _liveShoppingService.AddCommentAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/reactions")]
    public async Task<ActionResult<List<ReactionSummaryDto>>> AddReaction(Guid id, AddLiveReactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _liveShoppingService.AddReactionAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/buy")]
    public async Task<ActionResult<CartItemDto>> BuyDuringLive(Guid id, BuyDuringLiveRequest request, CancellationToken cancellationToken)
    {
        var result = await _liveShoppingService.BuyDuringLiveAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }
}

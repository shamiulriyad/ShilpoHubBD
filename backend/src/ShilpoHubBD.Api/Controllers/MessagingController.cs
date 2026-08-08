using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Messaging;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/messaging")]
[Authorize]
public class MessagingController : ControllerBase
{
    private readonly IMessagingService _messagingService;

    public MessagingController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("conversations")]
    public async Task<ActionResult<PagedResult<ConversationListItemDto>>> GetConversations(
        [FromQuery] ConversationQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _messagingService.GetConversationsAsync(CurrentUserId, query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("conversations/{id:guid}")]
    public async Task<ActionResult<ConversationDto>> GetConversationById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _messagingService.GetConversationByIdAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDto>> StartConversation(StartConversationRequest request, CancellationToken cancellationToken)
    {
        var result = await _messagingService.StartConversationAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetConversationById), new { id = result.Id }, result);
    }

    [HttpPost("conversations/{id:guid}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(Guid id, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _messagingService.SendMessageAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("conversations/{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _messagingService.MarkAsReadAsync(id, CurrentUserId, cancellationToken);
        return NoContent();
    }
}

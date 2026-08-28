using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Mentorship;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/mentorship-requests")]
[Authorize]
public class MentorshipRequestsController : ControllerBase
{
    private readonly IMentorshipService _mentorshipService;

    public MentorshipRequestsController(IMentorshipService mentorshipService)
    {
        _mentorshipService = mentorshipService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<MentorshipRequestDto>> Create(CreateMentorshipRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _mentorshipService.CreateRequestAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MentorshipRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mentorshipService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine/as-learner")]
    public async Task<ActionResult<List<MentorshipRequestListItemDto>>> GetMineAsLearner(CancellationToken cancellationToken)
    {
        var result = await _mentorshipService.GetMyRequestsAsLearnerAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine/as-mentor")]
    public async Task<ActionResult<List<MentorshipRequestListItemDto>>> GetMineAsMentor(CancellationToken cancellationToken)
    {
        var result = await _mentorshipService.GetMyRequestsAsMentorAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<MentorshipRequestDto>> Accept(
        Guid id, RespondMentorshipRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _mentorshipService.AcceptAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<MentorshipRequestDto>> Reject(
        Guid id, RespondMentorshipRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _mentorshipService.RejectAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<MentorshipRequestDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mentorshipService.CompleteAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }
}

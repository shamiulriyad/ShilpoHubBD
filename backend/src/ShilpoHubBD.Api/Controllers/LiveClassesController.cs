using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.LiveClass;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/live-classes")]
public class LiveClassesController : ControllerBase
{
    private readonly ILiveClassService _liveClassService;

    public LiveClassesController(ILiveClassService liveClassService)
    {
        _liveClassService = liveClassService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<LiveClassListItemDto>>> GetPaged(
        [FromQuery] LiveClassQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LiveClassDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<LiveClassListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _liveClassService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("registered")]
    public async Task<ActionResult<List<LiveClassListItemDto>>> GetRegistered(CancellationToken cancellationToken)
    {
        var result = await _liveClassService.GetRegisteredAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<LiveClassDto>> Create(CreateLiveClassRequest request, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LiveClassDto>> Update(Guid id, UpdateLiveClassRequest request, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<LiveClassDto>> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.StartAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/end")]
    public async Task<ActionResult<LiveClassDto>> End(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.EndAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<LiveClassDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.CancelAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _liveClassService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/register")]
    public async Task<ActionResult<LiveClassParticipantDto>> Register(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.RegisterAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id, CancellationToken cancellationToken)
    {
        await _liveClassService.JoinAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken cancellationToken)
    {
        await _liveClassService.LeaveAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("{id:guid}/attendance")]
    public async Task<ActionResult<List<LiveClassAttendanceDto>>> GetAttendance(Guid id, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.GetAttendanceAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/questions")]
    public async Task<ActionResult<LiveClassQuestionDto>> AskQuestion(Guid id, AskQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.AskQuestionAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/questions/{questionId:guid}/answer")]
    public async Task<ActionResult<LiveClassQuestionDto>> AnswerQuestion(
        Guid id, Guid questionId, AnswerQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _liveClassService.AnswerQuestionAsync(CurrentUserId, id, questionId, request, cancellationToken);
        return Ok(result);
    }
}

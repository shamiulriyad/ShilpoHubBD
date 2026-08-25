using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/program-applications")]
[Authorize]
public class ProgramApplicationsController : ControllerBase
{
    private readonly IProgramApplicationService _applicationService;

    public ProgramApplicationsController(IProgramApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<ProgramApplicationDto>> Apply(CreateProgramApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _applicationService.ApplyAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProgramApplicationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _applicationService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<ProgramApplicationListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _applicationService.GetMyApplicationsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("programs/{programId:guid}")]
    public async Task<ActionResult<List<ProgramApplicationListItemDto>>> GetByProgram(Guid programId, CancellationToken cancellationToken)
    {
        var result = await _applicationService.GetByProgramAsync(CurrentUserId, programId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<ProgramApplicationDto>> Accept(
        Guid id, RespondProgramApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _applicationService.AcceptAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ProgramApplicationDto>> Reject(
        Guid id, RespondProgramApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _applicationService.RejectAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<ProgramApplicationDto>> Withdraw(Guid id, CancellationToken cancellationToken)
    {
        var result = await _applicationService.WithdrawAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }
}

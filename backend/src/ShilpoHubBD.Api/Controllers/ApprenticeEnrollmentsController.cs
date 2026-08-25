using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/apprentice-enrollments")]
[Authorize]
public class ApprenticeEnrollmentsController : ControllerBase
{
    private readonly IApprenticeEnrollmentService _enrollmentService;

    public ApprenticeEnrollmentsController(IApprenticeEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("mine")]
    public async Task<ActionResult<List<ApprenticeEnrollmentListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetMyEnrollmentsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApprenticeEnrollmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("programs/{programId:guid}")]
    public async Task<ActionResult<List<ApprenticeEnrollmentListItemDto>>> GetByProgram(Guid programId, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetByProgramAsync(CurrentUserId, programId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/milestones/{milestoneId:guid}/progress")]
    public async Task<ActionResult<ApprenticeEnrollmentDto>> UpdateMilestoneProgress(
        Guid id, Guid milestoneId, UpdateMilestoneProgressRequest request, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.UpdateMilestoneProgressAsync(CurrentUserId, id, milestoneId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ApprenticeEnrollmentDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.CompleteAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }
}

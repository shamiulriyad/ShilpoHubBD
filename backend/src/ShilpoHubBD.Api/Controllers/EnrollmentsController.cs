using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpPost("courses/{courseId:guid}/enroll")]
    public async Task<ActionResult<CourseEnrollmentDto>> Enroll(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.EnrollAsync(CurrentUserId, courseId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<EnrollmentListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetMyEnrollmentsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseEnrollmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetEnrollmentAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("courses/{courseId:guid}")]
    public async Task<ActionResult<List<EnrollmentListItemDto>>> GetByCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetByCourseAsync(CurrentUserId, courseId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/progress")]
    public async Task<ActionResult<CourseEnrollmentDto>> MarkLessonProgress(
        Guid id, MarkLessonProgressRequest request, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.MarkLessonProgressAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<CourseEnrollmentDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.CompleteEnrollmentAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }
}

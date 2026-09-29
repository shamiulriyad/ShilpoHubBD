using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using System.Data;
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
    private readonly ShilpoHubDbContext _db;

    public EnrollmentsController(IEnrollmentService enrollmentService, ShilpoHubDbContext db)
    {
        _enrollmentService = enrollmentService;
        _db = db;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpPost("courses/{courseId:guid}/enroll")]
    public async Task<ActionResult<CourseEnrollmentDto>> Enroll(Guid courseId, CancellationToken cancellationToken, [FromQuery] string attendanceMode = "Online")
    {
        // Serialize seat allocation across concurrent requests and server instances.
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var result = await _enrollmentService.EnrollAsync(CurrentUserId, courseId, cancellationToken, attendanceMode);
            await transaction.CommitAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex) when (ex is Npgsql.PostgresException { SqlState: "40001" or "23505" } || ex.InnerException is Npgsql.PostgresException { SqlState: "40001" or "23505" })
        {
            return Conflict(new { message = "Another student reserved a seat at the same time. Please retry." });
        }
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

    [HttpGet("courses/{courseId:guid}")]
    public async Task<ActionResult<List<EnrollmentListItemDto>>> GetByCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetByCourseAsync(CurrentUserId, courseId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/progress")]
    public async Task<ActionResult<CourseEnrollmentDto>> MarkLessonProgress(
        Guid id, MarkLessonProgressRequest request, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.MarkLessonProgressAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<CourseEnrollmentDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.CompleteEnrollmentAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }
}

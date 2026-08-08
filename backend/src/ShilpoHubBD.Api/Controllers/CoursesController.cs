using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private Guid? CurrentUserIdOrNull => User.Identity?.IsAuthenticated == true ? CurrentUserId : null;

    [HttpGet]
    public async Task<ActionResult<PagedResult<CourseListItemDto>>> GetPublished(
        [FromQuery] CourseQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _courseService.GetPublishedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _courseService.GetByIdAsync(id, CurrentUserIdOrNull, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("mine")]
    public async Task<ActionResult<List<CourseListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _courseService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CourseDto>> Update(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<CourseDto>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _courseService.PublishAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<CourseDto>> Archive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _courseService.ArchiveAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/lessons")]
    public async Task<ActionResult<CourseLessonDto>> AddLesson(Guid id, CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.AddLessonAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPut("{id:guid}/lessons/{lessonId:guid}")]
    public async Task<ActionResult<CourseLessonDto>> UpdateLesson(
        Guid id, Guid lessonId, UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.UpdateLessonAsync(CurrentUserId, id, lessonId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpDelete("{id:guid}/lessons/{lessonId:guid}")]
    public async Task<IActionResult> DeleteLesson(Guid id, Guid lessonId, CancellationToken cancellationToken)
    {
        await _courseService.DeleteLessonAsync(CurrentUserId, id, lessonId, cancellationToken);
        return NoContent();
    }
}

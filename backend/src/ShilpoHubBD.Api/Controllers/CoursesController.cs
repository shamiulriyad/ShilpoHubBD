using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Services;

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

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<CourseListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _courseService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CourseDto>> Update(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<CourseDto>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _courseService.PublishAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<CourseDto>> Archive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _courseService.ArchiveAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _courseService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/lessons")]
    public async Task<ActionResult<CourseLessonDto>> AddLesson(Guid id, CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.AddLessonAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id:guid}/lessons/{lessonId:guid}")]
    public async Task<ActionResult<CourseLessonDto>> UpdateLesson(
        Guid id, Guid lessonId, UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.UpdateLessonAsync(CurrentUserId, id, lessonId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}/lessons/{lessonId:guid}")]
    public async Task<IActionResult> DeleteLesson(Guid id, Guid lessonId, CancellationToken cancellationToken)
    {
        await _courseService.DeleteLessonAsync(CurrentUserId, id, lessonId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/modules")]
    public async Task<ActionResult<CourseModuleDto>> AddModule(Guid id, CreateCourseModuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.AddModuleAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id:guid}/modules/{moduleId:guid}")]
    public async Task<ActionResult<CourseModuleDto>> UpdateModule(
        Guid id, Guid moduleId, UpdateCourseModuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.UpdateModuleAsync(CurrentUserId, id, moduleId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}/modules/{moduleId:guid}")]
    public async Task<IActionResult> DeleteModule(Guid id, Guid moduleId, CancellationToken cancellationToken)
    {
        await _courseService.DeleteModuleAsync(CurrentUserId, id, moduleId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/materials")]
    public async Task<ActionResult<CourseMaterialDto>> AddMaterial(Guid id, CreateCourseMaterialRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.AddMaterialAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}/materials/{materialId:guid}")]
    public async Task<IActionResult> DeleteMaterial(Guid id, Guid materialId, CancellationToken cancellationToken)
    {
        await _courseService.DeleteMaterialAsync(CurrentUserId, id, materialId, cancellationToken);
        return NoContent();
    }
}

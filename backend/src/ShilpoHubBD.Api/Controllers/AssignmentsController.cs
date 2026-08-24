using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Assessment;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("courses/{courseId:guid}")]
    public async Task<ActionResult<AssignmentDto>> Create(Guid courseId, CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.CreateAsync(CurrentUserId, courseId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("courses/{courseId:guid}")]
    public async Task<ActionResult<List<AssignmentListItemDto>>> GetByCourse(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GetByCourseAsync(CurrentUserId, courseId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssignmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssignmentDto>> Update(Guid id, UpdateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _assignmentService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<AssignmentSubmissionDto>> Submit(Guid id, SubmitAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.SubmitAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/my-submission")]
    public async Task<ActionResult<AssignmentSubmissionDto>> GetMySubmission(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GetMySubmissionAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/submissions")]
    public async Task<ActionResult<List<AssignmentSubmissionDto>>> GetSubmissions(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GetSubmissionsAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("submissions/{submissionId:guid}/grade")]
    public async Task<ActionResult<AssignmentSubmissionDto>> Grade(
        Guid submissionId, GradeAssignmentSubmissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GradeAsync(CurrentUserId, submissionId, request, cancellationToken);
        return Ok(result);
    }
}

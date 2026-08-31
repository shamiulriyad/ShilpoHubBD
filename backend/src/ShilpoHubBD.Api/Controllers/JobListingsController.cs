using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/job-listings")]
public class JobListingsController : ControllerBase
{
    private readonly IJobListingService _jobListingService;

    public JobListingsController(IJobListingService jobListingService)
    {
        _jobListingService = jobListingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private Guid? CurrentUserIdOrNull => User.Identity?.IsAuthenticated == true ? CurrentUserId : null;

    [HttpGet]
    public async Task<ActionResult<PagedResult<JobListingListItemDto>>> GetPublished(
        [FromQuery] JobListingQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _jobListingService.GetPublishedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobListingDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _jobListingService.GetByIdAsync(id, CurrentUserIdOrNull, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<JobListingListItemDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _jobListingService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<JobListingDto>> Create(CreateJobListingRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobListingService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobListingDto>> Update(Guid id, UpdateJobListingRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobListingService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<JobListingDto>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _jobListingService.PublishAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<JobListingDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _jobListingService.CloseAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _jobListingService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/skill-requirements")]
    public async Task<ActionResult<JobSkillRequirementDto>> AddSkillRequirement(
        Guid id, AddJobSkillRequirementRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobListingService.AddSkillRequirementAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}/skill-requirements/{requirementId:guid}")]
    public async Task<IActionResult> RemoveSkillRequirement(Guid id, Guid requirementId, CancellationToken cancellationToken)
    {
        await _jobListingService.RemoveSkillRequirementAsync(CurrentUserId, id, requirementId, cancellationToken);
        return NoContent();
    }
}

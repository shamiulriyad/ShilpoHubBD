using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.PerformingArts;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>Public Performing Arts discovery — Published records only, open to everyone (including anonymous visitors).</summary>
[ApiController]
[AllowAnonymous]
[EnableRateLimiting("read")]
[Route("api/performing-arts")]
public class PerformingArtsController : ControllerBase
{
    private readonly IPerformingArtService _service;

    public PerformingArtsController(IPerformingArtService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PerformingArtListItemDto>>> Search(
        [FromQuery] PerformingArtQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.SearchPublishedAsync(query, cancellationToken));

    [HttpGet("featured")]
    public async Task<ActionResult<List<PerformingArtListItemDto>>> Featured([FromQuery] int count = 6, CancellationToken cancellationToken = default)
        => Ok(await _service.GetFeaturedAsync(count, cancellationToken));

    [HttpGet("facets")]
    public async Task<ActionResult<PerformingArtFacetsDto>> Facets(CancellationToken cancellationToken)
        => Ok(await _service.GetFacetsAsync(cancellationToken));

    [HttpGet("{slug}")]
    public async Task<ActionResult<PerformingArtDetailDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        => Ok(await _service.GetPublishedBySlugAsync(slug, cancellationToken));
}

/// <summary>Editorial workflow for performing-arts records: create, edit, submit for review, publish, return to draft,
/// archive, restore and delete. Super Admin only.</summary>
[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/admin/performing-arts")]
public class AdminPerformingArtsController : ControllerBase
{
    private readonly IPerformingArtService _service;

    public AdminPerformingArtsController(IPerformingArtService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<PerformingArtListItemDto>>> Search(
        [FromQuery] PerformingArtQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.SearchAdminAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PerformingArtDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<PerformingArtDetailDto>> Create(SavePerformingArtRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, CurrentUserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PerformingArtDetailDto>> Update(Guid id, SavePerformingArtRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<PerformingArtDetailDto>> Submit(Guid id, PerformingArtWorkflowRequest request, CancellationToken cancellationToken)
        => Ok(await _service.SubmitForReviewAsync(id, CurrentUserId, request.Note, cancellationToken));

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<PerformingArtDetailDto>> Publish(Guid id, PerformingArtWorkflowRequest request, CancellationToken cancellationToken)
        => Ok(await _service.PublishAsync(id, CurrentUserId, request.Note, cancellationToken));

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<PerformingArtDetailDto>> ReturnToDraft(Guid id, PerformingArtWorkflowRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ReturnToDraftAsync(id, CurrentUserId, request.Note, cancellationToken));

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<PerformingArtDetailDto>> Archive(Guid id, PerformingArtWorkflowRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ArchiveAsync(id, CurrentUserId, request.Note, cancellationToken));

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<PerformingArtDetailDto>> Restore(Guid id, PerformingArtWorkflowRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RestoreAsync(id, CurrentUserId, request.Note, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

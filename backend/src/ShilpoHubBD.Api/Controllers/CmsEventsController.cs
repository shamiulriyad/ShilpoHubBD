using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/cms/events")]
public class CmsEventsController : ControllerBase
{
    private readonly ICmsEventService _service;

    public CmsEventsController(ICmsEventService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CmsEventListItemDto>>> GetPaged(
        [FromQuery] CmsEventQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(query, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet("drafts")]
    public async Task<ActionResult<PagedResult<CmsEventListItemDto>>> GetDrafts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default)
        => Ok(await _service.GetDraftsAsync(page, pageSize, cancellationToken));

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CmsEventDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        => Ok(await _service.GetBySlugAsync(slug, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CmsEventDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<CmsEventDto>> Create(CreateCmsEventRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CmsEventDto>> Update(Guid id, UpdateCmsEventRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

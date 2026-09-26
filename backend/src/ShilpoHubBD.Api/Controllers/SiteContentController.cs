using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/cms/site-content")]
public class SiteContentController : ControllerBase
{
    private readonly ISiteContentService _service;

    public SiteContentController(ISiteContentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<SiteContentItemDto>>> GetAll(
        [FromQuery] string? group, [FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(group, includeInactive && User.IsInRole(RoleNames.SuperAdmin), cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet("groups")]
    public ActionResult<string[]> GetGroups() => Ok(SiteContentGroups.All);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SiteContentItemDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<SiteContentItemDto>> Create(SaveSiteContentItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SiteContentItemDto>> Update(Guid id, SaveSiteContentItemRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

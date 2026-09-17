using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/cms/homepage")]
public class CmsHomepageController : ControllerBase
{
    private readonly IHomepageSectionService _service;

    public CmsHomepageController(IHomepageSectionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<HomepageSectionDto>>> GetAll(
        [FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(includeInactive, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HomepageSectionDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<HomepageSectionDto>> Create(CreateHomepageSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HomepageSectionDto>> Update(Guid id, UpdateHomepageSectionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

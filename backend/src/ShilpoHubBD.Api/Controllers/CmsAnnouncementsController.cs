using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/cms/announcements")]
public class CmsAnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _service;

    public CmsAnnouncementsController(IAnnouncementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAll(
        [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
        => Ok(await _service.GetAllAsync(activeOnly, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnnouncementDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<AnnouncementDto>> Create(CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AnnouncementDto>> Update(Guid id, UpdateAnnouncementRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/cultural-events")]
public class CulturalEventsController : ControllerBase
{
    private readonly ICulturalEventService _eventService;

    public CulturalEventsController(ICulturalEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CulturalEventDto>>> GetPaged(
        [FromQuery] CulturalEventQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _eventService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CulturalEventDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _eventService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<CulturalEventDto>> Create(CreateCulturalEventRequest request, CancellationToken cancellationToken)
    {
        var result = await _eventService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CulturalEventDto>> Update(Guid id, UpdateCulturalEventRequest request, CancellationToken cancellationToken)
        => Ok(await _eventService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _eventService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

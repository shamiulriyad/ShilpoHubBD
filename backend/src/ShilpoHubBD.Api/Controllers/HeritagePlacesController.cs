using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/heritage-places")]
public class HeritagePlacesController : ControllerBase
{
    private readonly IHeritagePlaceService _placeService;

    public HeritagePlacesController(IHeritagePlaceService placeService)
    {
        _placeService = placeService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<HeritagePlaceDto>>> GetPaged(
        [FromQuery] HeritagePlaceQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _placeService.GetPagedAsync(query, cancellationToken));

    [HttpGet("nearby")]
    public async Task<ActionResult<PagedResult<HeritagePlaceDto>>> GetNearby(
        [FromQuery] NearbyHeritagePlaceQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _placeService.GetNearbyAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HeritagePlaceDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _placeService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<HeritagePlaceDto>> Create(CreateHeritagePlaceRequest request, CancellationToken cancellationToken)
    {
        var result = await _placeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HeritagePlaceDto>> Update(Guid id, UpdateHeritagePlaceRequest request, CancellationToken cancellationToken)
        => Ok(await _placeService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _placeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

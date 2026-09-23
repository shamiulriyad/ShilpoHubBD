using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/tourism-locations")]
public class TourismLocationsController : ControllerBase
{
    private readonly ITourismLocationService _locationService;

    public TourismLocationsController(ITourismLocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TourismLocationDto>>> GetPaged(
        [FromQuery] TourismLocationQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _locationService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TourismLocationDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _locationService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<TourismLocationDto>> Create(CreateTourismLocationRequest request, CancellationToken cancellationToken)
    {
        var result = await _locationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TourismLocationDto>> Update(Guid id, UpdateTourismLocationRequest request, CancellationToken cancellationToken)
        => Ok(await _locationService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _locationService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPatch("{id:guid}/activate-status")]
    public async Task<ActionResult<TourismLocationDto>> SetActive(Guid id, SetTourismLocationActiveRequest request, CancellationToken cancellationToken)
        => Ok(await _locationService.SetActiveAsync(id, request.IsActive, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPatch("{id:guid}/verification")]
    public async Task<ActionResult<TourismLocationDto>> SetVerification(Guid id, SetTourismLocationVerificationRequest request, CancellationToken cancellationToken)
        => Ok(await _locationService.SetVerificationAsync(id, request.IsVerified, cancellationToken));
}

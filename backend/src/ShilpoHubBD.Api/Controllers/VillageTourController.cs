using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/village-tour-stops")]
public class VillageTourController : ControllerBase
{
    private readonly IVillageTourService _villageTourService;

    public VillageTourController(IVillageTourService villageTourService)
    {
        _villageTourService = villageTourService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<VillageTourStopDto>>> GetPaged(
        [FromQuery] VillageTourStopQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _villageTourService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VillageTourStopDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _villageTourService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<VillageTourStopDto>> Create(CreateVillageTourStopRequest request, CancellationToken cancellationToken)
    {
        var result = await _villageTourService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VillageTourStopDto>> Update(Guid id, UpdateVillageTourStopRequest request, CancellationToken cancellationToken)
        => Ok(await _villageTourService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _villageTourService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/heritage-festivals")]
public class HeritageFestivalsController : ControllerBase
{
    private readonly IHeritageFestivalService _festivalService;

    public HeritageFestivalsController(IHeritageFestivalService festivalService)
    {
        _festivalService = festivalService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<HeritageFestivalDto>>> GetPaged(
        [FromQuery] HeritageFestivalQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _festivalService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HeritageFestivalDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _festivalService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<HeritageFestivalDto>> Create(CreateHeritageFestivalRequest request, CancellationToken cancellationToken)
    {
        var result = await _festivalService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HeritageFestivalDto>> Update(Guid id, UpdateHeritageFestivalRequest request, CancellationToken cancellationToken)
        => Ok(await _festivalService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _festivalService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

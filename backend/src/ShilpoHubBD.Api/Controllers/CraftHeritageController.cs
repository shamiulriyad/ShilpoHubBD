using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/craft-heritage")]
public class CraftHeritageController : ControllerBase
{
    private readonly ICraftHeritageService _service;

    public CraftHeritageController(ICraftHeritageService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CraftHeritageEntryDto>>> GetAll(
        [FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(includeInactive && User.IsInRole(RoleNames.SuperAdmin), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CraftHeritageEntryDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<CraftHeritageEntryDto>> Create(SaveCraftHeritageEntryRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CraftHeritageEntryDto>> Update(Guid id, SaveCraftHeritageEntryRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/museum-items")]
public class MuseumItemsController : ControllerBase
{
    private readonly IMuseumItemService _museumItemService;

    public MuseumItemsController(IMuseumItemService museumItemService)
    {
        _museumItemService = museumItemService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MuseumItemDto>>> GetPaged(
        [FromQuery] MuseumItemQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _museumItemService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MuseumItemDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _museumItemService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<MuseumItemDto>> Create(CreateMuseumItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _museumItemService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MuseumItemDto>> Update(Guid id, UpdateMuseumItemRequest request, CancellationToken cancellationToken)
        => Ok(await _museumItemService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _museumItemService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

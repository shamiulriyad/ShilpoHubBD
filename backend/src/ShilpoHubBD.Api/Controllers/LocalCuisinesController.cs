using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/local-cuisines")]
public class LocalCuisinesController : ControllerBase
{
    private readonly ILocalCuisineService _cuisineService;

    public LocalCuisinesController(ILocalCuisineService cuisineService)
    {
        _cuisineService = cuisineService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<LocalCuisineDto>>> GetPaged(
        [FromQuery] LocalCuisineQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _cuisineService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocalCuisineDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _cuisineService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<LocalCuisineDto>> Create(CreateLocalCuisineRequest request, CancellationToken cancellationToken)
    {
        var result = await _cuisineService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LocalCuisineDto>> Update(Guid id, UpdateLocalCuisineRequest request, CancellationToken cancellationToken)
        => Ok(await _cuisineService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _cuisineService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

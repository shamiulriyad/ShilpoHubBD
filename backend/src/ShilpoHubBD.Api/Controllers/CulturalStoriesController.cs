using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/cultural-stories")]
public class CulturalStoriesController : ControllerBase
{
    private readonly ICulturalStoryService _culturalStoryService;

    public CulturalStoriesController(ICulturalStoryService culturalStoryService)
    {
        _culturalStoryService = culturalStoryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CulturalStoryDto>>> GetPaged(
        [FromQuery] CulturalStoryQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _culturalStoryService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CulturalStoryDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _culturalStoryService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<CulturalStoryDto>> Create(CreateCulturalStoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _culturalStoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CulturalStoryDto>> Update(Guid id, UpdateCulturalStoryRequest request, CancellationToken cancellationToken)
        => Ok(await _culturalStoryService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _culturalStoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

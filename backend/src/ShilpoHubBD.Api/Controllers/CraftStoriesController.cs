using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/craft-stories")]
public class CraftStoriesController : ControllerBase
{
    private readonly ICraftStoryService _craftStoryService;

    public CraftStoriesController(ICraftStoryService craftStoryService)
    {
        _craftStoryService = craftStoryService;
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<CraftStoryDto>> GetByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await _craftStoryService.GetByCategoryIdAsync(categoryId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<CraftStoryDto>> Create(CreateCraftStoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _craftStoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByCategory), new { categoryId = result.CategoryId }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CraftStoryDto>> Update(Guid id, UpdateCraftStoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _craftStoryService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _craftStoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

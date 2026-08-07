using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Community;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/villages")]
public class VillagesController : ControllerBase
{
    private readonly IVillageService _villageService;

    public VillagesController(IVillageService villageService)
    {
        _villageService = villageService;
    }

    private Guid? CurrentUserIdOrNull
    {
        get
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return sub is null ? null : Guid.Parse(sub);
        }
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<VillageDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _villageService.GetAllAsync(CurrentUserIdOrNull, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("favorites")]
    public async Task<ActionResult<List<VillageDto>>> GetMyFavorites(CancellationToken cancellationToken)
    {
        var result = await _villageService.GetMyFavoritesAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VillageDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _villageService.GetByIdAsync(id, CurrentUserIdOrNull, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<VillageDto>> Create(CreateVillageRequest request, CancellationToken cancellationToken)
    {
        var result = await _villageService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VillageDto>> Update(Guid id, UpdateVillageRequest request, CancellationToken cancellationToken)
    {
        var result = await _villageService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _villageService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/favorite")]
    public async Task<IActionResult> Favorite(Guid id, CancellationToken cancellationToken)
    {
        await _villageService.FavoriteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/favorite")]
    public async Task<IActionResult> Unfavorite(Guid id, CancellationToken cancellationToken)
    {
        await _villageService.UnfavoriteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }
}

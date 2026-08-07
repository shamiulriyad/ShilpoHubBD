using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/producers/{producerId:guid}/workshop-gallery")]
public class WorkshopGalleryController : ControllerBase
{
    private readonly IWorkshopGalleryService _workshopGalleryService;

    public WorkshopGalleryController(IWorkshopGalleryService workshopGalleryService)
    {
        _workshopGalleryService = workshopGalleryService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<List<WorkshopGalleryItemDto>>> GetByProducer(Guid producerId, CancellationToken cancellationToken)
    {
        var result = await _workshopGalleryService.GetByProducerIdAsync(producerId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<WorkshopGalleryItemDto>> Add(Guid producerId, CreateWorkshopGalleryItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _workshopGalleryService.AddAsync(producerId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> Delete(Guid producerId, Guid itemId, CancellationToken cancellationToken)
    {
        await _workshopGalleryService.DeleteAsync(producerId, itemId, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}

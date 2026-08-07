using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/producer-stories")]
public class ProducerStoriesController : ControllerBase
{
    private readonly IProducerStoryService _producerStoryService;

    public ProducerStoriesController(IProducerStoryService producerStoryService)
    {
        _producerStoryService = producerStoryService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("{producerId:guid}")]
    public async Task<ActionResult<ProducerStoryDto>> GetByProducer(Guid producerId, CancellationToken cancellationToken)
    {
        var result = await _producerStoryService.GetByProducerIdAsync(producerId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{producerId:guid}")]
    public async Task<ActionResult<ProducerStoryDto>> Upsert(Guid producerId, UpsertProducerStoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _producerStoryService.UpsertAsync(producerId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{producerId:guid}")]
    public async Task<IActionResult> Delete(Guid producerId, CancellationToken cancellationToken)
    {
        await _producerStoryService.DeleteAsync(producerId, cancellationToken);
        return NoContent();
    }
}

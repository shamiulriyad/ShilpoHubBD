using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>
/// Producers entered into one auction round as bid-able lots. A lot never exposes raw
/// producer/user fields to a Business Partner — only the Part 2 Business Profile aggregate.
/// </summary>
[ApiController]
[Route("api/producer-partnership-auctions/{auctionId:guid}/lots")]
[Authorize]
public class ProducerPartnershipAuctionLotsController : ControllerBase
{
    private readonly IProducerPartnershipAuctionLotService _lotService;

    public ProducerPartnershipAuctionLotsController(IProducerPartnershipAuctionLotService lotService)
    {
        _lotService = lotService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<ProducerPartnershipAuctionLotDetailDto>> AddLot(
        Guid auctionId, [FromBody] AddProducerPartnershipAuctionLotRequest request, CancellationToken cancellationToken)
    {
        var result = await _lotService.AddLotAsync(auctionId, request.ProducerId, CurrentUserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { auctionId, lotId = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{lotId:guid}")]
    public async Task<IActionResult> RemoveLot(Guid auctionId, Guid lotId, CancellationToken cancellationToken)
    {
        await _lotService.RemoveLotAsync(auctionId, lotId, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<List<ProducerPartnershipAuctionLotListItemDto>>> GetLots(Guid auctionId, CancellationToken cancellationToken)
        => Ok(await _lotService.GetLotsForAuctionAsync(auctionId, CurrentUserId, IsAdmin, cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet("{lotId:guid}")]
    public async Task<ActionResult<ProducerPartnershipAuctionLotDetailDto>> GetById(Guid auctionId, Guid lotId, CancellationToken cancellationToken)
        => Ok(await _lotService.GetLotDetailAsync(auctionId, lotId, CurrentUserId, IsAdmin, cancellationToken));
}

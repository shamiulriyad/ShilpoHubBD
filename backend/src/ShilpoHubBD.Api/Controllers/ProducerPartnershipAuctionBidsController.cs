using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/producer-partnership-auctions/{auctionId:guid}")]
[Authorize]
public class ProducerPartnershipAuctionBidsController : ControllerBase
{
    private readonly IProducerPartnershipAuctionBidService _bidService;

    public ProducerPartnershipAuctionBidsController(IProducerPartnershipAuctionBidService bidService)
    {
        _bidService = bidService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = RoleNames.BusinessPartner)]
    [HttpPost("lots/{lotId:guid}/bids")]
    public async Task<ActionResult<ProducerPartnershipAuctionBidDto>> PlaceBid(
        Guid auctionId, Guid lotId, PlaceProducerPartnershipAuctionBidRequest request, CancellationToken cancellationToken)
    {
        var result = await _bidService.PlaceBidAsync(auctionId, lotId, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet("lots/{lotId:guid}/bids")]
    public async Task<ActionResult<List<ProducerPartnershipAuctionBidHistoryEntryDto>>> GetBidHistory(
        Guid auctionId, Guid lotId, CancellationToken cancellationToken)
        => Ok(await _bidService.GetBidHistoryAsync(auctionId, lotId, cancellationToken));

    [Authorize(Roles = RoleNames.BusinessPartner)]
    [HttpGet("my-bids")]
    public async Task<ActionResult<List<ProducerPartnershipAuctionBidDto>>> GetMyBids(
        Guid auctionId, [FromQuery] Guid? lotId, CancellationToken cancellationToken)
        => Ok(await _bidService.GetMyBidsAsync(auctionId, lotId, CurrentUserId, cancellationToken));
}

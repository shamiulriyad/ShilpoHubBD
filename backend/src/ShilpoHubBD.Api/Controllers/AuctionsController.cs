using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Auction;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionService _auctionService;

    public AuctionsController(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<AuctionListItemDto>>> GetAll(
        [FromQuery] AuctionQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _auctionService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuctionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _auctionService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> Create(CreateAuctionRequest request, CancellationToken cancellationToken)
    {
        var result = await _auctionService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPost("{id:guid}/bids")]
    public async Task<ActionResult<AuctionDto>> PlaceBid(Guid id, PlaceBidRequest request, CancellationToken cancellationToken)
    {
        var result = await _auctionService.PlaceBidAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<AuctionDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _auctionService.CancelAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}

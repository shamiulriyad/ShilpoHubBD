using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>
/// Admin configuration and lifecycle for the annual Producer Partnership Auction. Reading an
/// auction's own details is open to eligible Business Partners (for the countdown/status view);
/// every other action here — creating, configuring and moving it through its lifecycle — is
/// SuperAdmin-only. Lots, bids and participant registration live in their own controllers.
/// </summary>
[ApiController]
[Route("api/producer-partnership-auctions")]
[Authorize]
public class ProducerPartnershipAuctionsController : ControllerBase
{
    private readonly IProducerPartnershipAuctionService _auctionService;

    public ProducerPartnershipAuctionsController(IProducerPartnershipAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<ProducerPartnershipAuctionDto>> Create(
        CreateProducerPartnershipAuctionRequest request, CancellationToken cancellationToken)
    {
        var result = await _auctionService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProducerPartnershipAuctionListItemDto>>> GetPaged(
        [FromQuery] ProducerPartnershipAuctionQueryParameters parameters, CancellationToken cancellationToken)
        => Ok(await _auctionService.GetPagedAsync(parameters, cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProducerPartnershipAuctionDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _auctionService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProducerPartnershipAuctionDto>> Update(
        Guid id, UpdateProducerPartnershipAuctionRequest request, CancellationToken cancellationToken)
        => Ok(await _auctionService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/schedule")]
    public async Task<ActionResult<ProducerPartnershipAuctionDto>> Schedule(Guid id, CancellationToken cancellationToken)
        => Ok(await _auctionService.ScheduleAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/open-registration")]
    public async Task<ActionResult<ProducerPartnershipAuctionDto>> OpenRegistration(Guid id, CancellationToken cancellationToken)
        => Ok(await _auctionService.OpenRegistrationAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/go-live")]
    public async Task<ActionResult<ProducerPartnershipAuctionDto>> GoLive(Guid id, CancellationToken cancellationToken)
        => Ok(await _auctionService.GoLiveAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/end")]
    public async Task<ActionResult<ProducerPartnershipAuctionDto>> End(Guid id, CancellationToken cancellationToken)
        => Ok(await _auctionService.EndAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ProducerPartnershipAuctionDto>> Cancel(Guid id, CancellationToken cancellationToken)
        => Ok(await _auctionService.CancelAsync(id, cancellationToken));
}

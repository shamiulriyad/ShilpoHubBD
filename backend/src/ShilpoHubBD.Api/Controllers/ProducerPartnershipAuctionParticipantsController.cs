using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>A Business Partner's registration to participate (bid) in one auction round, subject to admin approval.</summary>
[ApiController]
[Route("api/producer-partnership-auctions/{auctionId:guid}/participants")]
[Authorize]
public class ProducerPartnershipAuctionParticipantsController : ControllerBase
{
    private readonly IProducerPartnershipAuctionParticipantService _participantService;

    public ProducerPartnershipAuctionParticipantsController(IProducerPartnershipAuctionParticipantService participantService)
    {
        _participantService = participantService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = RoleNames.BusinessPartner)]
    [HttpPost]
    public async Task<ActionResult<ProducerPartnershipAuctionParticipantDto>> Apply(Guid auctionId, CancellationToken cancellationToken)
    {
        var result = await _participantService.ApplyAsync(auctionId, CurrentUserId, cancellationToken);
        return CreatedAtAction(nameof(GetMine), new { auctionId }, result);
    }

    [Authorize(Roles = RoleNames.BusinessPartner)]
    [HttpGet("me")]
    public async Task<ActionResult<ProducerPartnershipAuctionParticipantDto?>> GetMine(Guid auctionId, CancellationToken cancellationToken)
        => Ok(await _participantService.GetMineAsync(auctionId, CurrentUserId, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet]
    public async Task<ActionResult<List<ProducerPartnershipAuctionParticipantDto>>> GetForAuction(Guid auctionId, CancellationToken cancellationToken)
        => Ok(await _participantService.GetForAuctionAsync(auctionId, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{participantId:guid}/decide")]
    public async Task<ActionResult<ProducerPartnershipAuctionParticipantDto>> Decide(
        Guid auctionId, Guid participantId, DecideProducerPartnershipAuctionParticipantRequest request, CancellationToken cancellationToken)
        => Ok(await _participantService.DecideAsync(auctionId, participantId, CurrentUserId, request, cancellationToken));
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/manufacturing-partnerships")]
[Authorize]
public class ManufacturingPartnershipsController : ControllerBase
{
    private readonly IPartnershipService _partnershipService;

    public ManufacturingPartnershipsController(IPartnershipService partnershipService)
    {
        _partnershipService = partnershipService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<PartnershipDto>> Create(CreatePartnershipRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<PartnershipListItemDto>>> GetMine(
        [FromQuery] PartnershipQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.GetForBusinessPartnerAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("received")]
    public async Task<ActionResult<PagedResult<PartnershipListItemDto>>> GetReceived(
        [FromQuery] PartnershipQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.GetForProducerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PartnershipDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/respond")]
    public async Task<ActionResult<PartnershipDto>> Respond(Guid id, PartnershipResponseRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.RespondAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/milestones")]
    public async Task<ActionResult<MilestoneDto>> AddMilestone(Guid id, MilestoneInput request, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.AddMilestoneAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/milestones/{milestoneId:guid}/status")]
    public async Task<ActionResult<MilestoneDto>> UpdateMilestoneStatus(
        Guid id, Guid milestoneId, UpdateMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.UpdateMilestoneStatusAsync(id, milestoneId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<PartnershipDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.CompleteAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<PartnershipDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnershipService.CancelAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}

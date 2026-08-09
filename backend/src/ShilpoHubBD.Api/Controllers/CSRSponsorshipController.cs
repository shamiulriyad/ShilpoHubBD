using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/csr-sponsorship")]
[Authorize]
public class CSRSponsorshipController : ControllerBase
{
    private readonly ICSRSponsorshipService _sponsorshipService;

    public CSRSponsorshipController(ICSRSponsorshipService sponsorshipService)
    {
        _sponsorshipService = sponsorshipService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("opportunities")]
    public async Task<ActionResult<OpportunityDto>> CreateOpportunity(CreateOpportunityRequest request, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.CreateOpportunityAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetOpportunityById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("opportunities")]
    public async Task<ActionResult<PagedResult<OpportunityDto>>> GetOpportunities(
        [FromQuery] OpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.GetOpportunitiesAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("opportunities/mine")]
    public async Task<ActionResult<PagedResult<OpportunityDto>>> GetMyOpportunities(
        [FromQuery] OpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.GetOpportunitiesForProducerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("opportunities/{id:guid}")]
    public async Task<ActionResult<OpportunityDto>> GetOpportunityById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.GetOpportunityByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("opportunities/{id:guid}/close")]
    public async Task<ActionResult<OpportunityDto>> CloseOpportunity(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.CloseOpportunityAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("opportunities/{id:guid}/cancel")]
    public async Task<ActionResult<OpportunityDto>> CancelOpportunity(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.CancelOpportunityAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("opportunities/{id:guid}/proposals")]
    public async Task<ActionResult<PagedResult<ProposalListItemDto>>> GetProposalsForOpportunity(
        Guid id, [FromQuery] ProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.GetProposalsForOpportunityAsync(id, CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("opportunities/{id:guid}/proposals")]
    public async Task<ActionResult<ProposalDto>> SubmitProposal(Guid id, SubmitProposalRequest request, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.SubmitProposalAsync(id, CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetProposalById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet("proposals")]
    public async Task<ActionResult<PagedResult<ProposalListItemDto>>> GetMyProposals(
        [FromQuery] ProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.GetProposalsForBusinessPartnerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("proposals/{id:guid}")]
    public async Task<ActionResult<ProposalDto>> GetProposalById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.GetProposalByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/decision")]
    public async Task<ActionResult<ProposalDto>> DecideProposal(Guid id, ProposalDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.DecideProposalAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/milestones")]
    public async Task<ActionResult<SponsorshipMilestoneDto>> AddMilestone(Guid id, SponsorshipMilestoneInput request, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.AddMilestoneAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/milestones/{milestoneId:guid}/status")]
    public async Task<ActionResult<SponsorshipMilestoneDto>> UpdateMilestoneStatus(
        Guid id, Guid milestoneId, UpdateMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.UpdateMilestoneStatusAsync(id, milestoneId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/progress-updates")]
    public async Task<ActionResult<ProgressUpdateDto>> AddProgressUpdate(Guid id, AddProgressUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.AddProgressUpdateAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/impact-records")]
    public async Task<ActionResult<ImpactRecordDto>> AddImpactRecord(Guid id, AddImpactRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.AddImpactRecordAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/complete")]
    public async Task<ActionResult<ProposalDto>> CompleteProposal(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.CompleteProposalAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/cancel")]
    public async Task<ActionResult<ProposalDto>> CancelProposal(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sponsorshipService.CancelProposalAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}

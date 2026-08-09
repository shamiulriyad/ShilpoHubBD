using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Investment;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/investment-opportunities")]
[Authorize]
public class InvestmentOpportunitiesController : ControllerBase
{
    private readonly IInvestmentService _investmentService;

    public InvestmentOpportunitiesController(IInvestmentService investmentService)
    {
        _investmentService = investmentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost]
    public async Task<ActionResult<InvestmentOpportunityDto>> CreateOpportunity(CreateInvestmentOpportunityRequest request, CancellationToken cancellationToken)
    {
        var result = await _investmentService.CreateOpportunityAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetOpportunityById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<InvestmentOpportunityDto>>> GetOpportunities(
        [FromQuery] InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _investmentService.GetOpportunitiesAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResult<InvestmentOpportunityDto>>> GetMyOpportunities(
        [FromQuery] InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _investmentService.GetOpportunitiesForProducerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvestmentOpportunityDto>> GetOpportunityById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _investmentService.GetOpportunityByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<InvestmentOpportunityDto>> CloseOpportunity(Guid id, CancellationToken cancellationToken)
    {
        var result = await _investmentService.CloseOpportunityAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<InvestmentOpportunityDto>> CancelOpportunity(Guid id, CancellationToken cancellationToken)
    {
        var result = await _investmentService.CancelOpportunityAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}/proposals")]
    public async Task<ActionResult<PagedResult<InvestmentProposalListItemDto>>> GetProposalsForOpportunity(
        Guid id, [FromQuery] InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _investmentService.GetProposalsForOpportunityAsync(id, CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/proposals")]
    public async Task<ActionResult<InvestmentProposalDto>> SubmitProposal(Guid id, SubmitInvestmentProposalRequest request, CancellationToken cancellationToken)
    {
        var result = await _investmentService.SubmitProposalAsync(id, CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetProposalById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet("proposals/mine")]
    public async Task<ActionResult<PagedResult<InvestmentProposalListItemDto>>> GetMyProposals(
        [FromQuery] InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _investmentService.GetProposalsForBusinessPartnerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("proposals/{id:guid}")]
    public async Task<ActionResult<InvestmentProposalDto>> GetProposalById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _investmentService.GetProposalByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/decision")]
    public async Task<ActionResult<InvestmentProposalDto>> DecideProposal(Guid id, InvestmentProposalDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _investmentService.DecideProposalAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/milestones")]
    public async Task<ActionResult<InvestmentMilestoneDto>> AddMilestone(Guid id, InvestmentMilestoneInput request, CancellationToken cancellationToken)
    {
        var result = await _investmentService.AddMilestoneAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/milestones/{milestoneId:guid}/status")]
    public async Task<ActionResult<InvestmentMilestoneDto>> UpdateMilestoneStatus(
        Guid id, Guid milestoneId, UpdateInvestmentMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _investmentService.UpdateMilestoneStatusAsync(id, milestoneId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/documents")]
    public async Task<ActionResult<InvestmentDocumentDto>> AddDocument(Guid id, AddInvestmentDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _investmentService.AddDocumentAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/complete")]
    public async Task<ActionResult<InvestmentProposalDto>> CompleteProposal(Guid id, CancellationToken cancellationToken)
    {
        var result = await _investmentService.CompleteProposalAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("proposals/{id:guid}/cancel")]
    public async Task<ActionResult<InvestmentProposalDto>> CancelProposal(Guid id, CancellationToken cancellationToken)
    {
        var result = await _investmentService.CancelProposalAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}

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
/// Partnership revenue settlement: calculation + record-keeping + admin approval only. There is no
/// payout integration in this project, so nothing here ever claims money was actually transferred.
/// </summary>
[ApiController]
[Route("api/producer-partnership-settlements")]
[Authorize]
public class ProducerPartnershipSettlementsController : ControllerBase
{
    private readonly IProducerPartnershipSettlementService _settlementService;

    public ProducerPartnershipSettlementsController(IProducerPartnershipSettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("agreements/{agreementId:guid}/generate")]
    public async Task<ActionResult<ProducerPartnershipSettlementDto>> Generate(
        Guid agreementId, GenerateProducerPartnershipSettlementRequest request, CancellationToken cancellationToken)
    {
        var result = await _settlementService.GenerateAsync(agreementId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProducerPartnershipSettlementDto>>> GetPaged(
        [FromQuery] ProducerPartnershipSettlementQueryParameters parameters, CancellationToken cancellationToken)
        => Ok(await _settlementService.GetPagedAsync(parameters, cancellationToken));

    /// <summary>Platform revenue (commission) figure for the Admin dashboard.</summary>
    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet("platform-revenue-summary")]
    public async Task<ActionResult<PlatformRevenueSummaryDto>> GetPlatformRevenueSummary(CancellationToken cancellationToken)
        => Ok(await _settlementService.GetPlatformRevenueSummaryAsync(cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("agreements/{agreementId:guid}")]
    public async Task<ActionResult<List<ProducerPartnershipSettlementDto>>> GetForAgreement(Guid agreementId, CancellationToken cancellationToken)
        => Ok(await _settlementService.GetForAgreementAsync(agreementId, CurrentUserId, IsAdmin, cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProducerPartnershipSettlementDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _settlementService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/submit-for-approval")]
    public async Task<ActionResult<ProducerPartnershipSettlementDto>> SubmitForApproval(Guid id, CancellationToken cancellationToken)
        => Ok(await _settlementService.SubmitForApprovalAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ProducerPartnershipSettlementDto>> Approve(Guid id, CancellationToken cancellationToken)
        => Ok(await _settlementService.ApproveAsync(id, CurrentUserId, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ProducerPartnershipSettlementDto>> Reject(
        Guid id, RejectProducerPartnershipSettlementRequest request, CancellationToken cancellationToken)
        => Ok(await _settlementService.RejectAsync(id, request, cancellationToken));
}

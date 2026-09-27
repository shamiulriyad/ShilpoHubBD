using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>
/// The auction-winner → active-partnership workflow. Agreements are normally created automatically
/// (Pending) when an auction ends; from there an admin sets the revenue-share terms, then both the
/// producer and the Business Partner must confirm before it activates.
/// </summary>
[ApiController]
[Route("api/producer-partnership-agreements")]
[Authorize]
public class ProducerPartnershipAgreementsController : ControllerBase
{
    private readonly IProducerPartnershipAgreementService _agreementService;

    public ProducerPartnershipAgreementsController(IProducerPartnershipAgreementService agreementService)
    {
        _agreementService = agreementService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> Create(
        CreateProducerPartnershipAgreementRequest request, CancellationToken cancellationToken)
    {
        var result = await _agreementService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProducerPartnershipAgreementListItemDto>>> GetMine(
        [FromQuery] ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken)
        => Ok(await _agreementService.GetForBusinessPartnerAsync(CurrentUserId, IsAdmin, parameters, cancellationToken));

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("received")]
    public async Task<ActionResult<PagedResult<ProducerPartnershipAgreementListItemDto>>> GetReceived(
        [FromQuery] ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken)
        => Ok(await _agreementService.GetForProducerAsync(CurrentUserId, parameters, cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _agreementService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}/terms")]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> UpdateTerms(
        Guid id, UpdateProducerPartnershipAgreementTermsRequest request, CancellationToken cancellationToken)
        => Ok(await _agreementService.UpdateTermsAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/submit-for-confirmation")]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> SubmitForConfirmation(Guid id, CancellationToken cancellationToken)
        => Ok(await _agreementService.SubmitForConfirmationAsync(id, cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer}")]
    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> Confirm(Guid id, CancellationToken cancellationToken)
        => Ok(await _agreementService.ConfirmAsync(id, CurrentUserId, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/suspend")]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> Suspend(
        Guid id, TerminateProducerPartnershipAgreementRequest request, CancellationToken cancellationToken)
        => Ok(await _agreementService.SuspendAsync(id, request.Reason, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/resume")]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> Resume(Guid id, CancellationToken cancellationToken)
        => Ok(await _agreementService.ResumeAsync(id, cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> Complete(Guid id, CancellationToken cancellationToken)
        => Ok(await _agreementService.CompleteAsync(id, CurrentUserId, IsAdmin, cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ProducerPartnershipAgreementDto>> Cancel(
        Guid id, TerminateProducerPartnershipAgreementRequest request, CancellationToken cancellationToken)
        => Ok(await _agreementService.CancelAsync(id, CurrentUserId, IsAdmin, request, cancellationToken));

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}/settlement-eligibility")]
    public async Task<ActionResult<object>> GetSettlementEligibility(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _agreementService.EnsureCanRecordSettlementAsync(id, cancellationToken);
            return Ok(new { eligible = true, reason = (string?)null });
        }
        catch (ConflictException ex)
        {
            return Ok(new { eligible = false, reason = ex.Message });
        }
    }
}

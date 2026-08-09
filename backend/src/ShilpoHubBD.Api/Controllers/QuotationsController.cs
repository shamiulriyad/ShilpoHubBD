using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Quotations;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/quotations")]
[Authorize]
public class QuotationsController : ControllerBase
{
    private readonly IQuotationService _quotationService;

    public QuotationsController(IQuotationService quotationService)
    {
        _quotationService = quotationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<QuotationRequestDto>> Create(CreateQuotationRequest request, CancellationToken cancellationToken)
    {
        var result = await _quotationService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<QuotationRequestListItemDto>>> GetMine(
        [FromQuery] QuotationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _quotationService.GetForBusinessPartnerAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("received")]
    public async Task<ActionResult<PagedResult<QuotationRequestListItemDto>>> GetReceived(
        [FromQuery] QuotationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _quotationService.GetForProducerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuotationRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (IsAdmin || User.IsInRole(RoleNames.BusinessPartner))
        {
            var result = await _quotationService.GetByIdForBusinessPartnerAsync(id, CurrentUserId, IsAdmin, cancellationToken);
            return Ok(result);
        }

        var producerResult = await _quotationService.GetByIdForProducerAsync(id, CurrentUserId, cancellationToken);
        return Ok(producerResult);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}/compare")]
    public async Task<ActionResult<List<QuotationResponseDto>>> Compare(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quotationService.CompareAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/responses")]
    public async Task<ActionResult<QuotationResponseDto>> SubmitResponse(
        Guid id, SubmitQuotationResponseRequest request, CancellationToken cancellationToken)
    {
        var result = await _quotationService.SubmitResponseAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/responses/{responseId:guid}/decision")]
    public async Task<ActionResult<QuotationResponseDto>> DecideResponse(
        Guid id, Guid responseId, QuotationResponseDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _quotationService.DecideResponseAsync(id, responseId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<QuotationRequestDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _quotationService.CancelAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}

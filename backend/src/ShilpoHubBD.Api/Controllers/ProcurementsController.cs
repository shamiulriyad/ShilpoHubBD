using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Procurement;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/procurements")]
[Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
public class ProcurementsController : ControllerBase
{
    private readonly IProcurementService _procurementService;

    public ProcurementsController(IProcurementService procurementService)
    {
        _procurementService = procurementService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpPost]
    public async Task<ActionResult<ProcurementRequestDto>> Create(CreateProcurementRequest request, CancellationToken cancellationToken)
    {
        var result = await _procurementService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("from-quotation/{quotationResponseId:guid}")]
    public async Task<ActionResult<ProcurementRequestDto>> CreateFromQuotation(
        Guid quotationResponseId, CreateProcurementFromQuotationRequest request, CancellationToken cancellationToken)
    {
        var result = await _procurementService.CreateFromQuotationResponseAsync(CurrentUserId, IsAdmin, quotationResponseId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProcurementRequestListItemDto>>> GetMine(
        [FromQuery] ProcurementQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _procurementService.GetForBusinessPartnerAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProcurementRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _procurementService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ProcurementRequestDto>> Approve(Guid id, ProcurementDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _procurementService.ApproveAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ProcurementRequestDto>> Reject(Guid id, ProcurementDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _procurementService.RejectAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/convert-to-order")]
    public async Task<ActionResult<ProcurementRequestDto>> ConvertToOrder(Guid id, CancellationToken cancellationToken)
    {
        var result = await _procurementService.ConvertToOrderAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ProcurementRequestDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _procurementService.CancelAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}

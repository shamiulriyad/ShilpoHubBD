using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Contracts;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<ContractDto>> Create(CreateContractRequest request, CancellationToken cancellationToken)
    {
        var result = await _contractService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ContractListItemDto>>> GetMine(
        [FromQuery] ContractQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _contractService.GetForBusinessPartnerAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("received")]
    public async Task<ActionResult<PagedResult<ContractListItemDto>>> GetReceived(
        [FromQuery] ContractQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _contractService.GetForProducerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContractDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contractService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<ContractDto>> Accept(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contractService.AcceptAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ContractDto>> Reject(Guid id, ContractDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _contractService.RejectAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/terminate")]
    public async Task<ActionResult<ContractDto>> Terminate(Guid id, ContractDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _contractService.TerminateAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/renew")]
    public async Task<ActionResult<ContractDto>> Renew(Guid id, RenewContractRequest request, CancellationToken cancellationToken)
    {
        var result = await _contractService.RenewAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/documents")]
    public async Task<ActionResult<ContractDocumentDto>> AddDocument(Guid id, AddContractDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _contractService.AddDocumentAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/delivery-schedule/{scheduleId:guid}/status")]
    public async Task<ActionResult<ContractDeliveryScheduleDto>> UpdateDeliveryStatus(
        Guid id, Guid scheduleId, UpdateDeliveryStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _contractService.UpdateDeliveryStatusAsync(id, scheduleId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }
}

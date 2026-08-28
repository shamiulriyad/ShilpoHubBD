using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.CustomOrders;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/custom-orders")]
[Authorize]
public class CustomOrdersController : ControllerBase
{
    private readonly ICustomOrderService _customOrderService;

    public CustomOrdersController(ICustomOrderService customOrderService)
    {
        _customOrderService = customOrderService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomOrderRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customOrderService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine/customer")]
    public async Task<ActionResult<List<CustomOrderRequestDto>>> GetMineAsCustomer(CancellationToken cancellationToken)
    {
        var result = await _customOrderService.GetMineAsCustomerAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("mine/producer")]
    public async Task<ActionResult<List<CustomOrderRequestDto>>> GetMineAsProducer(CancellationToken cancellationToken)
    {
        var result = await _customOrderService.GetMineAsProducerAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomOrderRequestDto>> Create(CreateCustomOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _customOrderService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/respond")]
    public async Task<ActionResult<CustomOrderRequestDto>> Respond(Guid id, RespondToCustomOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _customOrderService.RespondAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<CustomOrderRequestDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customOrderService.CancelAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}

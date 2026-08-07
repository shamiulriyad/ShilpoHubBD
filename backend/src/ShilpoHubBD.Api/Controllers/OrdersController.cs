using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderListItemDto>>> GetMyOrders([FromQuery] OrderQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetMyOrdersAsync(CurrentUserId, query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/tracking")]
    public async Task<ActionResult<OrderTrackingDto>> GetTracking(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetTrackingAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout(CheckoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CheckoutAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancelOrderRequest? request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CancelAsync(id, CurrentUserId, IsAdmin, request ?? new CancelOrderRequest(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<OrderDto>> RequestReturn(Guid id, ReturnOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.RequestReturnAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<OrderDto>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.ConfirmAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/ship")]
    public async Task<ActionResult<OrderDto>> Ship(Guid id, ShipOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.ShipAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/deliver")]
    public async Task<ActionResult<OrderDto>> Deliver(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.DeliverAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/return/approve")]
    public async Task<ActionResult<OrderDto>> ApproveReturn(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.ApproveReturnAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/return/reject")]
    public async Task<ActionResult<OrderDto>> RejectReturn(Guid id, RejectReturnRequest? request, CancellationToken cancellationToken)
    {
        var result = await _orderService.RejectReturnAsync(id, request ?? new RejectReturnRequest(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<OrderDto>> Refund(Guid id, RefundOrderRequest? request, CancellationToken cancellationToken)
    {
        var result = await _orderService.RefundAsync(id, request ?? new RefundOrderRequest(), cancellationToken);
        return Ok(result);
    }
}

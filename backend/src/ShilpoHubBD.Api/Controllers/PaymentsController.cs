using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Initiate(InitiatePaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentService.InitiateAsync(request.OrderId, CurrentUserId, IsAdmin, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<List<PaymentDto>>> GetByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByOrderIdAsync(orderId, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/verify")]
    public async Task<ActionResult<PaymentDto>> Verify(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.VerifyAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    // Called by the payment gateway itself, not the logged-in user.
    [AllowAnonymous]
    [HttpPost("{id:guid}/callback")]
    public async Task<ActionResult<PaymentDto>> Callback(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        var result = await _paymentService.HandleCallbackAsync(id, payload.GetRawText(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<PaymentDto>> Refund(Guid id, RefundPaymentRequest? request, CancellationToken cancellationToken)
    {
        var result = await _paymentService.RefundAsync(id, request ?? new RefundPaymentRequest(), cancellationToken);
        return Ok(result);
    }
}

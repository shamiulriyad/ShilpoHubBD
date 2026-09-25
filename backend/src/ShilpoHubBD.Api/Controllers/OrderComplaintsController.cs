using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Complaints;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/order-complaints")]
[Authorize]
public class OrderComplaintsController : ControllerBase
{
    private readonly IOrderComplaintService _service;

    public OrderComplaintsController(IOrderComplaintService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<OrderComplaintDto>> Create(CreateOrderComplaintRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CreateAsync(CurrentUserId, request, cancellationToken));

    [HttpGet("mine")]
    public async Task<ActionResult<List<OrderComplaintDto>>> Mine(CancellationToken cancellationToken)
        => Ok(await _service.GetMineAsCustomerAsync(CurrentUserId, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("received")]
    public async Task<ActionResult<List<OrderComplaintDto>>> Received(CancellationToken cancellationToken)
        => Ok(await _service.GetMineAsProducerAsync(CurrentUserId, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/respond")]
    public async Task<ActionResult<OrderComplaintDto>> Respond(Guid id, RespondToOrderComplaintRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RespondAsync(id, CurrentUserId, request, cancellationToken));

    [HttpPost("{id:guid}/satisfied")]
    public async Task<ActionResult<OrderComplaintDto>> Satisfied(Guid id, CustomerComplaintNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ConfirmSatisfiedAsync(id, CurrentUserId, request, cancellationToken));

    [HttpPost("{id:guid}/reopen")]
    public async Task<ActionResult<OrderComplaintDto>> Reopen(Guid id, CustomerComplaintNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ReopenAsync(id, CurrentUserId, request, cancellationToken));

    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<OrderComplaintDto>> Withdraw(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.WithdrawAsync(id, CurrentUserId, cancellationToken));
}

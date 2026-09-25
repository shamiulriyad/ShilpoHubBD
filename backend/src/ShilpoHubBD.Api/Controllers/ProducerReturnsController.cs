using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/producer/returns")]
[Authorize(Roles = RoleNames.Producer)]
public class ProducerReturnsController : ControllerBase
{
    private readonly IProducerReturnService _service;

    public ProducerReturnsController(IProducerReturnService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record RejectReturnBody(string? Note);

    [HttpGet]
    public async Task<ActionResult<List<ProducerReturnDto>>> GetReturns(CancellationToken cancellationToken)
        => Ok(await _service.GetReturnsAsync(CurrentUserId, cancellationToken));

    [HttpPost("{orderId:guid}/accept")]
    public async Task<ActionResult<ProducerReturnDto>> Accept(Guid orderId, CancellationToken cancellationToken)
        => Ok(await _service.AcceptAsync(CurrentUserId, orderId, cancellationToken));

    [HttpPost("{orderId:guid}/reject")]
    public async Task<ActionResult<ProducerReturnDto>> Reject(Guid orderId, RejectReturnBody body, CancellationToken cancellationToken)
        => Ok(await _service.RejectAsync(CurrentUserId, orderId, body.Note, cancellationToken));
}

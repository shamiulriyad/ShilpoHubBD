using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/admin/security/threats")]
public class ThreatDetectionController : ControllerBase
{
    private readonly IThreatDetectionService _service;

    public ThreatDetectionController(IThreatDetectionService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("failed-logins")]
    public async Task<ActionResult<PagedResult<LoginAttemptDto>>> GetFailedLogins(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _service.GetFailedLoginsAsync(page, pageSize, cancellationToken));

    [HttpGet("suspicious-ips")]
    public async Task<ActionResult<List<SuspiciousIpDto>>> GetSuspiciousIps(CancellationToken cancellationToken)
        => Ok(await _service.GetSuspiciousIpsAsync(cancellationToken));

    [HttpGet("blocked-ips")]
    public async Task<ActionResult<List<BlockedIpDto>>> GetBlockedIps(CancellationToken cancellationToken)
        => Ok(await _service.GetBlockedIpsAsync(cancellationToken));

    [HttpPost("blocked-ips")]
    public async Task<ActionResult<BlockedIpDto>> BlockIp(BlockIpRequest request, CancellationToken cancellationToken)
        => Ok(await _service.BlockIpAsync(CurrentUserId, request, cancellationToken));

    [HttpDelete("blocked-ips")]
    public async Task<IActionResult> UnblockIp([FromQuery] string ipAddress, CancellationToken cancellationToken)
    {
        await _service.UnblockIpAsync(ipAddress, cancellationToken);
        return NoContent();
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/identity-verifications")]
public class IdentityVerificationsController : ControllerBase
{
    private readonly IIdentityVerificationService _verificationService;

    public IdentityVerificationsController(IIdentityVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<IdentityVerificationDto>> Submit(
        SubmitIdentityVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _verificationService.SubmitAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<List<IdentityVerificationDto>>> GetMine(CancellationToken cancellationToken)
        => Ok(await _verificationService.GetMineAsync(CurrentUserId, cancellationToken));

    [HttpGet]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<PagedResult<IdentityVerificationDto>>> GetPaged(
        [FromQuery] IdentityVerificationQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _verificationService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<IdentityVerificationDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _verificationService.GetByIdAsync(id, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<IdentityVerificationDto>> Approve(Guid id, CancellationToken cancellationToken)
        => Ok(await _verificationService.ApproveAsync(id, CurrentUserId, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<IdentityVerificationDto>> Reject(
        Guid id, RejectIdentityVerificationRequest request, CancellationToken cancellationToken)
        => Ok(await _verificationService.RejectAsync(id, CurrentUserId, request, cancellationToken));
}

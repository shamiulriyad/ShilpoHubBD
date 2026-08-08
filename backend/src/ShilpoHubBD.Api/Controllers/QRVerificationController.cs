using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.QRVerification;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/qr-verification")]
public class QRVerificationController : ControllerBase
{
    private readonly IQRVerificationService _qrVerificationService;

    public QRVerificationController(IQRVerificationService qrVerificationService)
    {
        _qrVerificationService = qrVerificationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private Guid? CurrentUserIdOrNull
    {
        get
        {
            var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpPost("verify")]
    public async Task<ActionResult<QRVerificationResultDto>> Verify(VerifyQRRequest request, CancellationToken cancellationToken)
    {
        var result = await _qrVerificationService.VerifyAsync(CurrentUserIdOrNull, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("generate")]
    public async Task<ActionResult<QRCodeDto>> Generate(GenerateQRCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _qrVerificationService.GenerateAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        await _qrVerificationService.RevokeAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("history/mine")]
    public async Task<ActionResult<PagedResult<QRVerificationHistoryItemDto>>> GetMyHistory(
        [FromQuery] QRVerificationQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _qrVerificationService.GetMyHistoryAsync(CurrentUserId, query, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("products/{productId:guid}/history")]
    public async Task<ActionResult<PagedResult<QRVerificationHistoryItemDto>>> GetProductHistory(
        Guid productId, [FromQuery] QRVerificationQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _qrVerificationService.GetProductHistoryAsync(productId, CurrentUserId, IsAdmin, query, cancellationToken);
        return Ok(result);
    }
}

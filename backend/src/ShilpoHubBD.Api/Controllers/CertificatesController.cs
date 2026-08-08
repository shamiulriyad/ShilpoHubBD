using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Certificate;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CertificateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _certificateService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var (fileName, html) = await _certificateService.GetDownloadAsync(id, cancellationToken);
        return File(Encoding.UTF8.GetBytes(html), "text/html", fileName);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("mine")]
    public async Task<ActionResult<List<CertificateDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _certificateService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("generate")]
    public async Task<ActionResult<CertificateDto>> Generate(GenerateCertificateRequest request, CancellationToken cancellationToken)
    {
        var result = await _certificateService.GenerateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        await _certificateService.RevokeAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }

    [HttpPost("verify")]
    public async Task<ActionResult<CertificateVerificationResultDto>> Verify(VerifyCertificateRequest request, CancellationToken cancellationToken)
    {
        var result = await _certificateService.VerifyAsync(request, cancellationToken);
        return Ok(result);
    }
}

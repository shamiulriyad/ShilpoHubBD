using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/training-certificates")]
public class TrainingCertificatesController : ControllerBase
{
    private readonly ITrainingCertificateService _trainingCertificateService;

    public TrainingCertificatesController(ITrainingCertificateService trainingCertificateService)
    {
        _trainingCertificateService = trainingCertificateService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingCertificateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _trainingCertificateService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var (fileName, html) = await _trainingCertificateService.GetDownloadAsync(id, cancellationToken);
        return File(Encoding.UTF8.GetBytes(html), "text/html", fileName);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<TrainingCertificateDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _trainingCertificateService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("verify")]
    public async Task<ActionResult<TrainingCertificateVerificationResultDto>> Verify(
        VerifyTrainingCertificateRequest request, CancellationToken cancellationToken)
    {
        var result = await _trainingCertificateService.VerifyAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("skill/issue")]
    public async Task<ActionResult<TrainingCertificateDto>> IssueSkillCertificate(
        IssueSkillCertificateRequest request, CancellationToken cancellationToken)
    {
        var result = await _trainingCertificateService.IssueSkillCertificateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        await _trainingCertificateService.RevokeAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}

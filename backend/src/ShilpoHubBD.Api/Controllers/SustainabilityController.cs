using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Sustainability;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/sustainability")]
public class SustainabilityController : ControllerBase
{
    private readonly ISustainabilityService _sustainabilityService;

    public SustainabilityController(ISustainabilityService sustainabilityService)
    {
        _sustainabilityService = sustainabilityService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("me")]
    public async Task<ActionResult<SustainabilityProfileDto>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _sustainabilityService.GetMyProfileAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("producers/{producerId:guid}")]
    public async Task<ActionResult<SustainabilityProfileDto>> GetByProducer(Guid producerId, CancellationToken cancellationToken)
    {
        var result = await _sustainabilityService.GetByProducerIdAsync(producerId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("materials")]
    public async Task<ActionResult<SustainableMaterialRecordDto>> AddMaterialRecord(
        CreateMaterialRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _sustainabilityService.AddMaterialRecordAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("certifications")]
    public async Task<ActionResult<SustainableMaterialCertificationDto>> AddCertification(
        CreateMaterialCertificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _sustainabilityService.AddCertificationAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("certifications/{certificationId:guid}/verify")]
    public async Task<ActionResult<SustainableMaterialCertificationDto>> VerifyCertification(
        Guid certificationId, CancellationToken cancellationToken)
    {
        var result = await _sustainabilityService.VerifyCertificationAsync(certificationId, cancellationToken);
        return Ok(result);
    }
}

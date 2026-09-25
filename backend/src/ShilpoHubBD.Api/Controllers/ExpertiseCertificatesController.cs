using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Certificates;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
public class ExpertiseCertificatesController : ControllerBase
{
    private readonly IExpertiseCertificateService _service;

    public ExpertiseCertificatesController(IExpertiseCertificateService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("api/expertise-certificates/mine")]
    public async Task<ActionResult<ExpertiseProgressDto>> Mine(CancellationToken cancellationToken)
        => Ok(await _service.GetMineAsync(CurrentUserId, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet("api/admin/expertise-certificates/eligible")]
    public async Task<ActionResult<List<EligibleProducerDto>>> Eligible(CancellationToken cancellationToken)
        => Ok(await _service.GetEligibleAsync(cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("api/admin/expertise-certificates/issue")]
    public async Task<ActionResult<ExpertiseCertificateDto>> Issue(IssueExpertiseCertificateRequest request, CancellationToken cancellationToken)
        => Ok(await _service.IssueAsync(request.ProducerId, CurrentUserId, cancellationToken));
}

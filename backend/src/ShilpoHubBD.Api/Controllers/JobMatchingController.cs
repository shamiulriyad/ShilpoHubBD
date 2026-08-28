using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/job-matching")]
[Authorize]
public class JobMatchingController : ControllerBase
{
    private readonly IJobMatchingService _jobMatchingService;

    public JobMatchingController(IJobMatchingService jobMatchingService)
    {
        _jobMatchingService = jobMatchingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("recommended")]
    public async Task<ActionResult<List<JobMatchResultDto>>> GetRecommended(JobMatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobMatchingService.GetRecommendedJobsAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }
}

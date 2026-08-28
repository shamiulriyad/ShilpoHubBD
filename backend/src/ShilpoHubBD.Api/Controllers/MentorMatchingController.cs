using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.MentorMatching;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/mentor-matching")]
[Authorize]
public class MentorMatchingController : ControllerBase
{
    private readonly IMentorMatchingService _mentorMatchingService;

    public MentorMatchingController(IMentorMatchingService mentorMatchingService)
    {
        _mentorMatchingService = mentorMatchingService;
    }

    [HttpPost("match")]
    public async Task<ActionResult<List<MentorMatchResultDto>>> Match(MentorMatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _mentorMatchingService.MatchAsync(request, cancellationToken);
        return Ok(result);
    }
}

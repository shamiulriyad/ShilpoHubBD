using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Portfolio;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/mentor-feedback")]
[Authorize]
public class MentorFeedbackController : ControllerBase
{
    private readonly IMentorFeedbackService _mentorFeedbackService;

    public MentorFeedbackController(IMentorFeedbackService mentorFeedbackService)
    {
        _mentorFeedbackService = mentorFeedbackService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<MentorFeedbackDto>> Submit(SubmitMentorFeedbackRequest request, CancellationToken cancellationToken)
    {
        var result = await _mentorFeedbackService.SubmitAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<MentorFeedbackDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _mentorFeedbackService.GetForLearnerAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}

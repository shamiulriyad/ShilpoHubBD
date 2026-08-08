using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/mentors")]
public class MentorsController : ControllerBase
{
    private readonly IMentorService _mentorService;

    public MentorsController(IMentorService mentorService)
    {
        _mentorService = mentorService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<MentorListItemDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default)
    {
        var result = await _mentorService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MentorProfileDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mentorService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("me")]
    public async Task<ActionResult<MentorProfileDto>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _mentorService.GetMyProfileAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost]
    public async Task<ActionResult<MentorProfileDto>> BecomeMentor(BecomeMentorRequest request, CancellationToken cancellationToken)
    {
        var result = await _mentorService.BecomeMentorAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPut("me")]
    public async Task<ActionResult<MentorProfileDto>> UpdateProfile(UpdateMentorProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _mentorService.UpdateProfileAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }
}

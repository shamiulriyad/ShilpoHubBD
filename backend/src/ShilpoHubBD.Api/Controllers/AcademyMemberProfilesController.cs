using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/academy-profiles")]
public class AcademyMemberProfilesController : ControllerBase
{
    private readonly IAcademyMemberProfileService _profileService;

    public AcademyMemberProfilesController(IAcademyMemberProfileService profileService)
    {
        _profileService = profileService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AcademyMemberProfileDto>> Create(CreateAcademyMemberProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _profileService.CreateProfileAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AcademyMemberProfileDto>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _profileService.GetMyProfileAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<AcademyMemberProfileDto>> UpdateMine(UpdateAcademyMemberProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _profileService.UpdateProfileAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AcademyMemberProfileDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _profileService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("me/skills")]
    public async Task<ActionResult<AcademyMemberProfileDto>> AddSkill(AddMemberSkillRequest request, CancellationToken cancellationToken)
    {
        var result = await _profileService.AddSkillAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("me/skills/{heritageSkillId:guid}")]
    public async Task<ActionResult<AcademyMemberProfileDto>> RemoveSkill(Guid heritageSkillId, CancellationToken cancellationToken)
    {
        var result = await _profileService.RemoveSkillAsync(CurrentUserId, heritageSkillId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me/learning-history")]
    public async Task<ActionResult<List<EnrollmentListItemDto>>> GetMyLearningHistory(CancellationToken cancellationToken)
    {
        var result = await _profileService.GetMyLearningHistoryAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}

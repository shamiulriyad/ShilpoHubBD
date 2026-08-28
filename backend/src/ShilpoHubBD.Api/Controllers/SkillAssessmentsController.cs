using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.SkillAssessment;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/skill-assessments")]
[Authorize]
public class SkillAssessmentsController : ControllerBase
{
    private readonly ISkillAssessmentService _skillAssessmentService;

    public SkillAssessmentsController(ISkillAssessmentService skillAssessmentService)
    {
        _skillAssessmentService = skillAssessmentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("skills/{heritageSkillId:guid}/run")]
    public async Task<ActionResult<SkillAssessmentResultDto>> Run(Guid heritageSkillId, CancellationToken cancellationToken)
    {
        var result = await _skillAssessmentService.RunAssessmentAsync(CurrentUserId, heritageSkillId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SkillAssessmentResultDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _skillAssessmentService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<SkillAssessmentListItemDto>>> GetHistory(CancellationToken cancellationToken)
    {
        var result = await _skillAssessmentService.GetHistoryAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Roadmap;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/learning-roadmaps")]
[Authorize]
public class LearningRoadmapsController : ControllerBase
{
    private readonly ILearningRoadmapService _roadmapService;

    public LearningRoadmapsController(ILearningRoadmapService roadmapService)
    {
        _roadmapService = roadmapService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<LearningRoadmapDto>> Create(CreateRoadmapRequest request, CancellationToken cancellationToken)
    {
        var result = await _roadmapService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<LearningRoadmapDto>> GetActive(CancellationToken cancellationToken)
    {
        var result = await _roadmapService.GetActiveAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LearningRoadmapDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roadmapService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<LearningRoadmapListItemDto>>> GetHistory(CancellationToken cancellationToken)
    {
        var result = await _roadmapService.GetHistoryAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/refresh")]
    public async Task<ActionResult<LearningRoadmapDto>> RefreshProgress(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roadmapService.RefreshProgressAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/milestones/{milestoneId:guid}/complete")]
    public async Task<ActionResult<LearningRoadmapDto>> CompleteMilestone(Guid id, Guid milestoneId, CancellationToken cancellationToken)
    {
        var result = await _roadmapService.CompleteMilestoneAsync(CurrentUserId, id, milestoneId, cancellationToken);
        return Ok(result);
    }
}

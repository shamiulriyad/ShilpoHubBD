using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Portfolio;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/portfolios")]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfoliosController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<PortfolioDto>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _portfolioService.GetMyPortfolioAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{academyMemberProfileId:guid}")]
    public async Task<ActionResult<PortfolioDto>> GetPublic(Guid academyMemberProfileId, CancellationToken cancellationToken)
    {
        var result = await _portfolioService.GetPublicPortfolioAsync(academyMemberProfileId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<PortfolioDto>> UpdateMine(UpdatePortfolioRequest request, CancellationToken cancellationToken)
    {
        var result = await _portfolioService.UpdateMyPortfolioAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("me/visibility")]
    public async Task<ActionResult<PortfolioDto>> UpdateVisibility(UpdatePortfolioVisibilityRequest request, CancellationToken cancellationToken)
    {
        var result = await _portfolioService.UpdateVisibilityAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("me/projects")]
    public async Task<ActionResult<PortfolioProjectDto>> AddProject(CreatePortfolioProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _portfolioService.AddProjectAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("me/projects/{projectId:guid}")]
    public async Task<ActionResult<PortfolioProjectDto>> UpdateProject(
        Guid projectId, UpdatePortfolioProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _portfolioService.UpdateProjectAsync(CurrentUserId, projectId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("me/projects/{projectId:guid}")]
    public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken cancellationToken)
    {
        await _portfolioService.DeleteProjectAsync(CurrentUserId, projectId, cancellationToken);
        return NoContent();
    }
}

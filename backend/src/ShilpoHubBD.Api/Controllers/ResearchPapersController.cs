using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/research/projects/{projectId:guid}/papers")]
public class ResearchPapersController : ControllerBase
{
    private readonly IResearchPaperService _paperService;

    public ResearchPapersController(IResearchPaperService paperService)
    {
        _paperService = paperService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ResearchPaperDto>>> GetForProject(
        Guid projectId, CancellationToken cancellationToken)
        => Ok(await _paperService.GetForProjectAsync(CurrentUserId, projectId, cancellationToken));

    [HttpGet("{paperId:guid}")]
    public async Task<ActionResult<ResearchPaperDto>> GetById(
        Guid projectId, Guid paperId, CancellationToken cancellationToken)
        => Ok(await _paperService.GetByIdAsync(CurrentUserId, projectId, paperId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ResearchPaperDto>> Create(
        Guid projectId, CreateResearchPaperRequest request, CancellationToken cancellationToken)
    {
        var result = await _paperService.CreateAsync(CurrentUserId, projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { projectId, paperId = result.Id }, result);
    }

    [HttpPut("{paperId:guid}")]
    public async Task<ActionResult<ResearchPaperDto>> Update(
        Guid projectId, Guid paperId, UpdateResearchPaperRequest request, CancellationToken cancellationToken)
        => Ok(await _paperService.UpdateAsync(CurrentUserId, projectId, paperId, request, cancellationToken));

    [HttpDelete("{paperId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid paperId, CancellationToken cancellationToken)
    {
        await _paperService.DeleteAsync(CurrentUserId, projectId, paperId, cancellationToken);
        return NoContent();
    }
}

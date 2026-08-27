using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/research/projects/{projectId:guid}/notes")]
public class ResearchNotesController : ControllerBase
{
    private readonly IResearchNoteService _noteService;

    public ResearchNotesController(IResearchNoteService noteService)
    {
        _noteService = noteService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ResearchNoteDto>>> GetForProject(
        Guid projectId, CancellationToken cancellationToken)
        => Ok(await _noteService.GetForProjectAsync(CurrentUserId, projectId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ResearchNoteDto>> Create(
        Guid projectId, CreateResearchNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _noteService.CreateAsync(CurrentUserId, projectId, request, cancellationToken));

    [HttpPut("{noteId:guid}")]
    public async Task<ActionResult<ResearchNoteDto>> Update(
        Guid projectId, Guid noteId, UpdateResearchNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _noteService.UpdateAsync(CurrentUserId, projectId, noteId, request, cancellationToken));

    [HttpDelete("{noteId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid noteId, CancellationToken cancellationToken)
    {
        await _noteService.DeleteAsync(CurrentUserId, projectId, noteId, cancellationToken);
        return NoContent();
    }
}

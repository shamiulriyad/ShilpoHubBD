using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/research/projects/{projectId:guid}/publications")]
public class ResearchPublicationsController : ControllerBase
{
    private readonly IResearchPublicationService _publicationService;

    public ResearchPublicationsController(IResearchPublicationService publicationService)
    {
        _publicationService = publicationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ResearchPublicationDto>>> GetForProject(
        Guid projectId, CancellationToken cancellationToken)
        => Ok(await _publicationService.GetForProjectAsync(CurrentUserId, projectId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ResearchPublicationDto>> Create(
        Guid projectId, CreateResearchPublicationRequest request, CancellationToken cancellationToken)
        => Ok(await _publicationService.CreateAsync(CurrentUserId, projectId, request, cancellationToken));

    [HttpPut("{publicationId:guid}")]
    public async Task<ActionResult<ResearchPublicationDto>> Update(
        Guid projectId, Guid publicationId, UpdateResearchPublicationRequest request, CancellationToken cancellationToken)
        => Ok(await _publicationService.UpdateAsync(CurrentUserId, projectId, publicationId, request, cancellationToken));

    [HttpDelete("{publicationId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid publicationId, CancellationToken cancellationToken)
    {
        await _publicationService.DeleteAsync(CurrentUserId, projectId, publicationId, cancellationToken);
        return NoContent();
    }
}

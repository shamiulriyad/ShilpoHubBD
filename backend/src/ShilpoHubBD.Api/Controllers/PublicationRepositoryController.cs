using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>
/// Cross-project publication repository: public publications plus any belonging to
/// projects the caller is a member of.
/// </summary>
[ApiController]
[Authorize]
[Route("api/research/publications")]
public class PublicationRepositoryController : ControllerBase
{
    private readonly IResearchPublicationService _publicationService;

    public PublicationRepositoryController(IResearchPublicationService publicationService)
    {
        _publicationService = publicationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ResearchPublicationDto>>> Browse(
        [FromQuery] ResearchPublicationQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _publicationService.BrowseAsync(CurrentUserId, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResearchPublicationDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _publicationService.GetByIdAsync(CurrentUserId, id, cancellationToken));
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.StoryGenerator;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>Cross-platform AI feature. Requires login (producers/admins draft stories with it) but no
/// specific role, matching "everyone uses" — unlike Heritage Assistant / Counterfeit Detection this isn't
/// useful to anonymous visitors, so it's gated to authenticated users only.</summary>
[ApiController]
[Authorize]
[Route("api/ai/story-generator")]
public class StoryGeneratorController : ControllerBase
{
    private readonly IStoryGeneratorService _service;

    public StoryGeneratorController(IStoryGeneratorService service)
    {
        _service = service;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GeneratedStoryDto>> Generate(GenerateCraftStoryRequest request, CancellationToken cancellationToken)
        => Ok(await _service.GenerateAsync(request, cancellationToken));
}

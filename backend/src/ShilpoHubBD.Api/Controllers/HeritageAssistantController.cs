using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.HeritageAssistant;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>Cross-platform AI feature — open to every role (and anonymous visitors), like AITourismController.</summary>
[ApiController]
[Route("api/ai/heritage-assistant")]
public class HeritageAssistantController : ControllerBase
{
    private readonly IHeritageAssistantService _service;

    public HeritageAssistantController(IHeritageAssistantService service)
    {
        _service = service;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<HeritageAssistantAnswerDto>> Ask(AskHeritageAssistantRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AskAsync(request, cancellationToken));
}

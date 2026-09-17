using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.CounterfeitDetection;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>Cross-platform AI feature — open to every role, so customers can check a listing before buying.</summary>
[ApiController]
[Route("api/ai/counterfeit-detection")]
public class CounterfeitDetectionController : ControllerBase
{
    private readonly ICounterfeitDetectionService _service;

    public CounterfeitDetectionController(ICounterfeitDetectionService service)
    {
        _service = service;
    }

    [HttpGet("check/{productId:guid}")]
    public async Task<ActionResult<CounterfeitCheckResultDto>> Check(Guid productId, CancellationToken cancellationToken)
        => Ok(await _service.CheckAsync(productId, cancellationToken));
}

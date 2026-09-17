using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.SentimentAnalysis;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>Cross-platform AI feature — open to every role, like AITourismController.</summary>
[ApiController]
[Route("api/ai/sentiment")]
public class SentimentAnalysisController : ControllerBase
{
    private readonly ISentimentAnalysisService _service;

    public SentimentAnalysisController(ISentimentAnalysisService service)
    {
        _service = service;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<SentimentResultDto>> Analyze(AnalyzeSentimentRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AnalyzeAsync(request, cancellationToken));

    [HttpGet("products/{productId:guid}")]
    public async Task<ActionResult<ProductSentimentSummaryDto>> GetProductSentiment(Guid productId, CancellationToken cancellationToken)
        => Ok(await _service.GetProductSentimentAsync(productId, cancellationToken));
}

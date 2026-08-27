using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/research/projects/{projectId:guid}/ai")]
public class ResearchAiAssistantController : ControllerBase
{
    private readonly IResearchAIService _aiService;

    public ResearchAiAssistantController(IResearchAIService aiService)
    {
        _aiService = aiService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("insights")]
    public async Task<ActionResult<ResearchAIAnalysisDto>> RunInsights(
        Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken)
        => Ok(await _aiService.RunInsightsAsync(CurrentUserId, projectId, request, cancellationToken));

    [HttpPost("trends")]
    public async Task<ActionResult<ResearchAIAnalysisDto>> RunTrendDiscovery(
        Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken)
        => Ok(await _aiService.RunTrendDiscoveryAsync(CurrentUserId, projectId, request, cancellationToken));

    [HttpPost("correlations")]
    public async Task<ActionResult<ResearchAIAnalysisDto>> RunCorrelationDetection(
        Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken)
        => Ok(await _aiService.RunCorrelationDetectionAsync(CurrentUserId, projectId, request, cancellationToken));

    [HttpPost("report")]
    public async Task<ActionResult<ResearchAIAnalysisDto>> RunReportGeneration(
        Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken)
        => Ok(await _aiService.RunReportGenerationAsync(CurrentUserId, projectId, request, cancellationToken));

    [HttpPost("citations")]
    public async Task<ActionResult<ResearchAIAnalysisDto>> GenerateCitations(
        Guid projectId, GenerateResearchCitationsRequest request, CancellationToken cancellationToken)
        => Ok(await _aiService.GenerateCitationsAsync(CurrentUserId, projectId, request, cancellationToken));

    [HttpGet("analyses")]
    public async Task<ActionResult<PagedResult<ResearchAIAnalysisListItemDto>>> GetAnalyses(
        Guid projectId, [FromQuery] ResearchAIAnalysisQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _aiService.GetForProjectAsync(CurrentUserId, projectId, query, cancellationToken));

    [HttpGet("analyses/{analysisId:guid}")]
    public async Task<ActionResult<ResearchAIAnalysisDto>> GetAnalysis(
        Guid projectId, Guid analysisId, CancellationToken cancellationToken)
        => Ok(await _aiService.GetByIdAsync(CurrentUserId, projectId, analysisId, cancellationToken));

    [HttpDelete("analyses/{analysisId:guid}")]
    public async Task<IActionResult> DeleteAnalysis(Guid projectId, Guid analysisId, CancellationToken cancellationToken)
    {
        await _aiService.DeleteAsync(CurrentUserId, projectId, analysisId, cancellationToken);
        return NoContent();
    }
}

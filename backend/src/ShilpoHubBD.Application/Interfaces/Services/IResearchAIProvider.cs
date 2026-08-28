using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Abstraction over the "intelligence" behind the AI Research Assistant. Every method is a pure
/// function of a pre-fetched context, so a future Gemini / OpenAI / custom-ML implementation can be
/// registered in place of <c>DummyResearchAIProvider</c> without touching <c>ResearchAIService</c>
/// or the controller.
/// </summary>
public interface IResearchAIProvider
{
    Task<ResearchAnalysisResult> GenerateInsightsAsync(ResearchAnalysisContext context, CancellationToken cancellationToken);

    Task<ResearchAnalysisResult> DiscoverTrendsAsync(ResearchAnalysisContext context, CancellationToken cancellationToken);

    Task<ResearchAnalysisResult> DetectCorrelationsAsync(ResearchAnalysisContext context, CancellationToken cancellationToken);

    Task<ResearchAnalysisResult> GenerateReportAsync(ResearchAnalysisContext context, CancellationToken cancellationToken);

    Task<ResearchCitationResult> GenerateCitationsAsync(ResearchCitationContext context, CancellationToken cancellationToken);
}

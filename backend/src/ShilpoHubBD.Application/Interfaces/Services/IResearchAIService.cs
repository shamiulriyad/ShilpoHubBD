using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IResearchAIService
{
    Task<ResearchAIAnalysisDto> RunInsightsAsync(
        Guid userId, Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken);

    Task<ResearchAIAnalysisDto> RunTrendDiscoveryAsync(
        Guid userId, Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken);

    Task<ResearchAIAnalysisDto> RunCorrelationDetectionAsync(
        Guid userId, Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken);

    Task<ResearchAIAnalysisDto> RunReportGenerationAsync(
        Guid userId, Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken);

    Task<ResearchAIAnalysisDto> GenerateCitationsAsync(
        Guid userId, Guid projectId, GenerateResearchCitationsRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ResearchAIAnalysisListItemDto>> GetForProjectAsync(
        Guid userId, Guid projectId, ResearchAIAnalysisQueryParameters query, CancellationToken cancellationToken);

    Task<ResearchAIAnalysisDto> GetByIdAsync(
        Guid userId, Guid projectId, Guid analysisId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid projectId, Guid analysisId, CancellationToken cancellationToken);
}

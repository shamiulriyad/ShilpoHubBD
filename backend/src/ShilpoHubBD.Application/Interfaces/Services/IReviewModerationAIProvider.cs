using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>Analyzes a single review's text. Implementations must never throw for an ordinary AI-unavailable
/// condition — they fall back to a deterministic result instead (see <see cref="ReviewModerationResultDto.IsAiGenerated"/>).</summary>
public interface IReviewModerationAIProvider
{
    Task<ReviewModerationResultDto> AnalyzeAsync(ReviewModerationContext context, CancellationToken cancellationToken);
}

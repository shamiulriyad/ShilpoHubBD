using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IArtisanSupportImpactService
{
    /// <summary>
    /// (Re)computes the before/after comparison for this case using its current SupportProvidedAt and
    /// whatever monthly reports/threshold configuration exist right now, replacing any previous
    /// assessment for this case. Throws ConflictException if support delivery hasn't been recorded yet.
    /// Reuses IArtisanSupportService.GetCaseAsync for the exact same view-permission rule the case
    /// itself uses (NotFoundException/UnauthorizedAccessException as appropriate).
    /// </summary>
    Task<ArtisanSupportImpactAssessmentDto> GenerateAsync(
        Guid userId, Guid caseId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken);

    /// <summary>Throws NotFoundException if none has been generated yet for this case.</summary>
    Task<ArtisanSupportImpactAssessmentDto> GetAsync(
        Guid userId, Guid caseId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken);

    /// <summary>
    /// One row per completed intervention (a case whose support has been provided) visible to the
    /// caller — same visibility rule as GetCasesAsync (a Producer sees only their own, a Government/NGO
    /// user only their organization's, an Admin sees all). Uses each case's persisted impact assessment
    /// when one exists; otherwise computes one live, without persisting it, purely for this report.
    /// </summary>
    Task<List<ProducerSupportImpactReportRowDto>> GetImpactReportAsync(
        Guid userId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken);

    /// <summary>
    /// Generates a short narrative interpretation of this case's existing impact assessment (via Gemini,
    /// or the rule-based fallback) and stores it as a new, separate historical record. Throws
    /// NotFoundException if no assessment has been generated for this case yet — the numbers must exist
    /// and be frozen before anything interprets them.
    /// </summary>
    Task<ProducerImpactAIAnalysisDto> GenerateAiSummaryAsync(
        Guid userId, Guid caseId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken);

    /// <summary>Throws NotFoundException if no narrative has been generated yet for this case.</summary>
    Task<ProducerImpactAIAnalysisDto> GetLatestAiSummaryAsync(
        Guid userId, Guid caseId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken);
}

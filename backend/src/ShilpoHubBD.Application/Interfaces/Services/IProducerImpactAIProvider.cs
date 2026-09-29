using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Interprets an already-computed ProducerImpactAiContext into a short narrative. Implementations must
/// never calculate a financial number themselves — every number in the context was already computed
/// and classified (Improved/Declined/NoSignificantChange/InsufficientData) before this is called.
/// </summary>
public interface IProducerImpactAIProvider
{
    Task<ProducerImpactNarrativeDto> GenerateNarrativeAsync(ProducerImpactAiContext context, CancellationToken cancellationToken);
}

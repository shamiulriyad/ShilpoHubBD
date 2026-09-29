using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>Compares a new negative review against retrieved historical reviews for the same product to decide
/// whether it's a repeated complaint. Never decides the product's risk state — that's deterministic backend
/// logic; this only classifies a single comparison.</summary>
public interface IRepeatedComplaintAIProvider
{
    Task<RepeatedComplaintResultDto> CompareAsync(RepeatedComplaintContext context, CancellationToken cancellationToken);
}

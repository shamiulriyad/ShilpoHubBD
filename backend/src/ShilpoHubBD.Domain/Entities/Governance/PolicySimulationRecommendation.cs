namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>An action the provider suggests based on a <see cref="PolicySimulation"/>'s result.</summary>
public class PolicySimulationRecommendation
{
    public Guid Id { get; set; }

    public Guid PolicySimulationId { get; set; }
    public PolicySimulation Simulation { get; set; } = null!;

    public PolicyRecommendationPriority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

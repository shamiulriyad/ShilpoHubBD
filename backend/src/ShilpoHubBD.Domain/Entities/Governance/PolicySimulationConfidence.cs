namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>How much weight to put on a projection, given how much real signal backed it.</summary>
public enum PolicySimulationConfidence
{
    Low,
    Moderate,
    High,
}

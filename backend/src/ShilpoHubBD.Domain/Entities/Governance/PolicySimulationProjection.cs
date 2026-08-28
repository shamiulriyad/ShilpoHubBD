namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>One projected metric outcome of a <see cref="PolicySimulation"/> over its horizon.</summary>
public class PolicySimulationProjection
{
    public Guid Id { get; set; }

    public Guid PolicySimulationId { get; set; }
    public PolicySimulation Simulation { get; set; } = null!;

    /// <summary>e.g. "Employment", "ExportRevenue", "TourismRevenue", "ActiveProducers", "HeritageEconomyValue".</summary>
    public string Metric { get; set; } = string.Empty;

    /// <summary>e.g. "people", "BDT", "count".</summary>
    public string Unit { get; set; } = string.Empty;

    public decimal BaselineValue { get; set; }
    public decimal ProjectedValue { get; set; }
    public decimal DeltaValue { get; set; }
    public double DeltaPercent { get; set; }

    public int HorizonMonths { get; set; }
    public PolicySimulationConfidence Confidence { get; set; }

    public string? Detail { get; set; }
    public int DisplayOrder { get; set; }
}

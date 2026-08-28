namespace ShilpoHubBD.Application.DTOs.Governance;

public class PolicySimulationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SimulationType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public Guid? ScopeId { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int HorizonMonths { get; set; }

    public string InputsJson { get; set; } = "{}";
    public string? AssumptionsJson { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;

    public int BaselineProducers { get; set; }
    public int BaselineActiveProducers { get; set; }
    public int BaselineEmployment { get; set; }
    public decimal BaselineExportValue { get; set; }
    public decimal BaselineTourismRevenue { get; set; }
    public decimal BaselineEconomyValue { get; set; }

    public string? Notes { get; set; }
    public string? FailureReason { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<PolicySimulationProjectionDto> Projections { get; set; } = new();
    public List<PolicySimulationRecommendationDto> Recommendations { get; set; } = new();
}

public class PolicySimulationProjectionDto
{
    public string Metric { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal BaselineValue { get; set; }
    public decimal ProjectedValue { get; set; }
    public decimal DeltaValue { get; set; }
    public double DeltaPercent { get; set; }
    public int HorizonMonths { get; set; }
    public string Confidence { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public int DisplayOrder { get; set; }
}

public class PolicySimulationRecommendationDto
{
    public string Priority { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class PolicySimulationListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SimulationType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string ScopeLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int HorizonMonths { get; set; }
    public string Confidence { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? GeneratedByName { get; set; }
}

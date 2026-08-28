namespace ShilpoHubBD.Application.DTOs.Governance;

public class ComputeHeritageIndexRequest
{
    /// <summary>
    /// HeritageRiskIndex, LivingHeritageIndex, CraftHealthScore, VillageSurvivalIndex,
    /// YouthParticipation or ClimateRiskAnalysis.
    /// </summary>
    public string IndexType { get; set; } = string.Empty;

    /// <summary>National, District, Village or Craft.</summary>
    public string Scope { get; set; } = "National";

    /// <summary>Required for District / Village scope: the district or village id.</summary>
    public Guid? ScopeId { get; set; }

    /// <summary>Required for Craft scope: the craft name to assess.</summary>
    public string? CraftLabel { get; set; }

    /// <summary>Optional aggregation window start; defaults to 12 months before <see cref="PeriodEnd"/>.</summary>
    public DateTime? PeriodStart { get; set; }

    /// <summary>Optional aggregation window end; defaults to now.</summary>
    public DateTime? PeriodEnd { get; set; }

    /// <summary>When false, the score is returned but not saved. Defaults to true.</summary>
    public bool Persist { get; set; } = true;

    public string? Notes { get; set; }
}

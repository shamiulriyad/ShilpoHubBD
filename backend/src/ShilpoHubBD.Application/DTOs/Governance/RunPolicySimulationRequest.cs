namespace ShilpoHubBD.Application.DTOs.Governance;

public class RunPolicySimulationRequest
{
    public string Title { get; set; } = string.Empty;

    /// <summary>GrantProgram, TrainingProgram, TourismCampaign, ExportStrategy or EmploymentPrediction.</summary>
    public string SimulationType { get; set; } = string.Empty;

    /// <summary>National, District, Village or Craft.</summary>
    public string Scope { get; set; } = "National";

    /// <summary>Required for District / Village scope.</summary>
    public Guid? ScopeId { get; set; }

    /// <summary>Projection horizon in months (3–120). Defaults to 12.</summary>
    public int? HorizonMonths { get; set; }

    // ---- Scenario knobs (all optional) --------------------------------
    public decimal? Budget { get; set; }
    public int? TargetBeneficiaries { get; set; }
    public int? DurationMonths { get; set; }
    public double? IntensityPercent { get; set; }
    public string? FocusCraft { get; set; }

    public string? Notes { get; set; }

    /// <summary>When false the result is returned but not saved. Defaults to true.</summary>
    public bool Persist { get; set; } = true;
}

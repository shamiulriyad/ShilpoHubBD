namespace ShilpoHubBD.Application.DTOs.Governance;

public class CreateNationalDashboardSnapshotRequest
{
    public string Label { get; set; } = string.Empty;

    /// <summary>Monthly, Quarterly, Yearly or Custom.</summary>
    public string Period { get; set; } = "Custom";

    /// <summary>Inclusive start of the window to aggregate. Required.</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>Exclusive end of the window to aggregate. Required and must be after <see cref="PeriodStart"/>.</summary>
    public DateTime PeriodEnd { get; set; }

    public string? Notes { get; set; }
}

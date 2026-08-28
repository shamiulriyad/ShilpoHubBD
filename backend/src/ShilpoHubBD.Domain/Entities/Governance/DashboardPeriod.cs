namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>The reporting cadence a <see cref="NationalDashboardSnapshot"/> represents.</summary>
public enum DashboardPeriod
{
    Monthly,
    Quarterly,
    Yearly,
    Custom,
}

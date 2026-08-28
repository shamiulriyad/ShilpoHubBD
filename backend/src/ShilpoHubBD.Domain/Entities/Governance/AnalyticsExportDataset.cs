namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>Which governance dataset a downloadable analytics export covers.</summary>
public enum AnalyticsExportDataset
{
    NationalDashboardSnapshots,
    HeritageIndexRecords,
    PolicySimulations,
    MonitoringFlags,
    Complaints,
    ComplianceRecords,
    FundingPrograms,
    FundingApplications,
    GovReport,
}

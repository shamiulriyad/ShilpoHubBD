namespace ShilpoHubBD.Application.DTOs.Governance;

public class AnalyticsExportDto
{
    public Guid Id { get; set; }
    public string Dataset { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FiltersJson { get; set; }
    public int? RowCount { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FailureReason { get; set; }
    public Guid? GovReportId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string? RequestedByName { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateAnalyticsExportRequest
{
    /// <summary>
    /// NationalDashboardSnapshots, HeritageIndexRecords, PolicySimulations, MonitoringFlags,
    /// Complaints, ComplianceRecords, FundingPrograms, FundingApplications or GovReport.
    /// </summary>
    public string Dataset { get; set; } = string.Empty;

    /// <summary>Csv, Json, Xlsx or Pdf.</summary>
    public string Format { get; set; } = "Csv";

    /// <summary>Free-form filter object (date range, type, status …); stored verbatim as JSON.</summary>
    public string? FiltersJson { get; set; }

    /// <summary>Required when Dataset is GovReport.</summary>
    public Guid? GovReportId { get; set; }
}

public class CompleteAnalyticsExportRequest
{
    /// <summary>Completed or Failed.</summary>
    public string Outcome { get; set; } = "Completed";
    public string? FileUrl { get; set; }
    public int? RowCount { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FailureReason { get; set; }
}

public class AnalyticsExportQueryParameters
{
    public string? Dataset { get; set; }
    public string? Status { get; set; }
    public string? Format { get; set; }
    public bool MineOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

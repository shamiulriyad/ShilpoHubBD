using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A request to export a governance dataset for download. The backend records metadata only — an
/// external worker produces the file and fills in <see cref="FileUrl"/> / <see cref="RowCount"/>.
/// </summary>
public class AnalyticsExport
{
    public Guid Id { get; set; }

    public AnalyticsExportDataset Dataset { get; set; }
    public AnalyticsExportFormat Format { get; set; }
    public AnalyticsExportStatus Status { get; set; } = AnalyticsExportStatus.Pending;

    /// <summary>Filters applied to the export (date range, type, status …), serialised as JSON.</summary>
    public string? FiltersJson { get; set; }

    public int? RowCount { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FailureReason { get; set; }

    /// <summary>Set when the export targets a single generated report.</summary>
    public Guid? GovReportId { get; set; }
    public GovReport? Report { get; set; }

    public Guid RequestedByUserId { get; set; }
    public User RequestedBy { get; set; } = null!;

    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

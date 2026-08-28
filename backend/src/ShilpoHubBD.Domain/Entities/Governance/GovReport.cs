using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A generated Government / NGO report (monthly, quarterly, annual or custom) that assembles a
/// point-in-time picture of the heritage economy, monitoring activity and funding pipeline for a
/// period. Section content is stored as JSON so the report is self-contained.
/// </summary>
public class GovReport
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public GovReportType ReportType { get; set; }
    public GovReportStatus Status { get; set; } = GovReportStatus.Draft;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public string Summary { get; set; } = string.Empty;
    public string? Highlights { get; set; }

    /// <summary>The full assembled dataset for the report, serialised as JSON.</summary>
    public string PayloadJson { get; set; } = "{}";

    public DateTime GeneratedAt { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public User GeneratedBy { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<GovReportSection> Sections { get; set; } = new List<GovReportSection>();
}

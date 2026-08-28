namespace ShilpoHubBD.Application.DTOs.Governance;

public class GenerateGovReportRequest
{
    public string Title { get; set; } = string.Empty;

    /// <summary>Monthly, Quarterly, Annual or Custom.</summary>
    public string ReportType { get; set; } = "Monthly";

    /// <summary>
    /// Optional. If omitted, the period is derived from <see cref="ReportType"/> relative to now
    /// (Monthly = last calendar month, Quarterly = last 3 months, Annual = last 12 months).
    /// </summary>
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }

    public string? Highlights { get; set; }

    /// <summary>Publish immediately instead of leaving as Draft.</summary>
    public bool Publish { get; set; }
}

public class UpdateGovReportRequest
{
    public string? Title { get; set; }

    /// <summary>Draft, Published or Archived.</summary>
    public string? Status { get; set; }
    public string? Highlights { get; set; }
    public string? Summary { get; set; }
}

public class GovReportQueryParameters
{
    public string? ReportType { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

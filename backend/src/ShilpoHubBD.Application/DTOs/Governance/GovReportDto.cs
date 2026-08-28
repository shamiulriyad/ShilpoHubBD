namespace ShilpoHubBD.Application.DTOs.Governance;

public class GovReportDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Highlights { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTime GeneratedAt { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<GovReportSectionDto> Sections { get; set; } = new();
}

public class GovReportSectionDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Narrative { get; set; }
    public string ContentJson { get; set; } = "{}";
    public int DisplayOrder { get; set; }
}

public class GovReportListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? GeneratedByName { get; set; }
    public DateTime? PublishedAt { get; set; }
}

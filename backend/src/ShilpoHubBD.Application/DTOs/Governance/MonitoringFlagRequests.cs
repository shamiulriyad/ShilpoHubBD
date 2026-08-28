namespace ShilpoHubBD.Application.DTOs.Governance;

public class CreateMonitoringFlagRequest
{
    /// <summary>FraudRisk, FakeProduct, ReviewAbuse, QrAnomaly, ComplianceGap or Other.</summary>
    public string FlagType { get; set; } = string.Empty;

    /// <summary>Info, Low, Medium, High or Critical.</summary>
    public string Severity { get; set; } = "Medium";

    /// <summary>Producer, Product, Order, Payment, QrCode, Review, Village, District or Other.</summary>
    public string SubjectType { get; set; } = "Other";
    public Guid? SubjectId { get; set; }
    public string SubjectLabel { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? RiskScore { get; set; }
}

public class UpdateMonitoringFlagStatusRequest
{
    /// <summary>Open, UnderReview, Confirmed, Dismissed or Resolved.</summary>
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class AssignMonitoringFlagRequest
{
    public Guid AssigneeUserId { get; set; }
    public string? Note { get; set; }
}

public class AddMonitoringFlagNoteRequest
{
    public string Note { get; set; } = string.Empty;
}

public class MonitoringFlagQueryParameters
{
    public string? FlagType { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? SubjectType { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

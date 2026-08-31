namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>One themed section of a <see cref="GovReport"/> (economy, monitoring, funding, …).</summary>
public class GovReportSection
{
    public Guid Id { get; set; }

    public Guid GovReportId { get; set; }
    public GovReport Report { get; set; } = null!;

    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    /// <summary>Short prose summary of the section.</summary>
    public string? Narrative { get; set; }

    /// <summary>Section figures, serialised as JSON.</summary>
    public string ContentJson { get; set; } = "{}";

    public int DisplayOrder { get; set; }
}

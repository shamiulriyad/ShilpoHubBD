namespace ShilpoHubBD.Application.DTOs.Governance;

public class NationalDashboardSnapshotQueryParameters
{
    /// <summary>Optional filter: Monthly, Quarterly, Yearly or Custom.</summary>
    public string? Period { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

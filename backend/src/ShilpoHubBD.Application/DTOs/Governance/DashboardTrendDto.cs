namespace ShilpoHubBD.Application.DTOs.Governance;

public class DashboardTrendDto
{
    public string Metric { get; set; } = string.Empty;
    public List<DashboardTrendPointDto> Points { get; set; } = new();
}

public class DashboardTrendPointDto
{
    public Guid SnapshotId { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime PeriodEnd { get; set; }
    public decimal Value { get; set; }

    /// <summary>Percent change from the previous point; null for the first point.</summary>
    public double? ChangePercent { get; set; }
}

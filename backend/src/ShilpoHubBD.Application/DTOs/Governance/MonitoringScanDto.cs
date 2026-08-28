namespace ShilpoHubBD.Application.DTOs.Governance;

public class RunMonitoringScanRequest
{
    /// <summary>Fraud, FakeProduct, ReviewAbuse, QrAnomaly or All. Defaults to All.</summary>
    public string ScanType { get; set; } = "All";

    /// <summary>Only consider activity on/after this date. Defaults to 180 days ago.</summary>
    public DateTime? Since { get; set; }

    /// <summary>Drop candidate findings below this heuristic risk score (0–100). Defaults to 40.</summary>
    public decimal? MinRiskScore { get; set; }
}

public class MonitoringScanResultDto
{
    public string ScanType { get; set; } = string.Empty;
    public DateTime RanAt { get; set; }
    public DateTime Since { get; set; }
    public int CandidatesEvaluated { get; set; }
    public int FlagsCreated { get; set; }
    public int DuplicatesSkipped { get; set; }
    public int BelowThresholdSkipped { get; set; }
    public List<MonitoringFlagListItemDto> CreatedFlags { get; set; } = new();
}

namespace ShilpoHubBD.Application.DTOs.Governance;

public class HeritageIndexTrendDto
{
    public string IndexType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string ScopeLabel { get; set; } = string.Empty;
    public List<HeritageIndexTrendPointDto> Points { get; set; } = new();
}

public class HeritageIndexTrendPointDto
{
    public Guid RecordId { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime ComputedAt { get; set; }
    public decimal Score { get; set; }
    public string Rating { get; set; } = string.Empty;

    /// <summary>Score-point change from the previous point; null for the first point.</summary>
    public decimal? ChangePoints { get; set; }
}

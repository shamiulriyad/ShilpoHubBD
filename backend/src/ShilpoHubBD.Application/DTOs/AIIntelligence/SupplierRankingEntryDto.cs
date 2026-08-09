namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class SupplierRankingEntryDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public decimal RankScore { get; set; }
    public decimal Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}

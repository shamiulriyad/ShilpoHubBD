namespace ShilpoHubBD.Application.DTOs.Governance;

public class DistrictRankingDto
{
    public int Rank { get; set; }
    public Guid DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;

    /// <summary>Which metric the ranking is by: producers, sales, products, villages or orders.</summary>
    public string Metric { get; set; } = string.Empty;

    public decimal Value { get; set; }
}

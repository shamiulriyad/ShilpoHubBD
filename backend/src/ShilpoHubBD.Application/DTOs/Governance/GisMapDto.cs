namespace ShilpoHubBD.Application.DTOs.Governance;

/// <summary>
/// District-keyed values for a chosen metric, ready to join to a client-side boundary file. No
/// geometry is stored on the platform, so this carries attributes only.
/// </summary>
public class GisMapDto
{
    public DateTime GeneratedAt { get; set; }
    public string Metric { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public List<GisDistrictPointDto> Districts { get; set; } = new();
}

public class GisDistrictPointDto
{
    public Guid DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public decimal Value { get; set; }

    /// <summary>Rank by value, 1 = highest.</summary>
    public int Rank { get; set; }
}

public class GisMapQueryParameters
{
    /// <summary>producers, products, villages, orders, sales or risk. Defaults to sales.</summary>
    public string? Metric { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

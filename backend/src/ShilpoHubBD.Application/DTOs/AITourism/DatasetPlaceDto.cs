namespace ShilpoHubBD.Application.DTOs.AITourism;

// One tourism entity from the 64-district dataset (retrieved through the Travel Planner RAG
// service). The dataset carries no coordinates, fees, hours or prices, so this has none either.
// Id is derived deterministically from Key so a model-chosen stop can be grounded like any DB id.
public class DatasetPlaceDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? EntityType { get; set; }
    public List<string> Interests { get; set; } = new();
    public List<string> MatchedInterests { get; set; } = new();
    public string? Description { get; set; }
}

public class DistrictDatasetResult
{
    public bool Found { get; set; }
    public List<DatasetPlaceDto> Places { get; set; } = new();
    // Selected interests the district has no entity for, and where the dataset suggests going instead.
    public List<string> UnmatchedInterests { get; set; } = new();
    public List<string> FallbackDistricts { get; set; } = new();
}

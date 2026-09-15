namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class CreateUnescoRecordRequest
{
    public string Title { get; set; } = string.Empty;

    /// <summary>CulturalHeritageSite, NaturalHeritageSite, IntangibleCulturalHeritage or MemoryOfTheWorld.</summary>
    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public int InscribedYear { get; set; }
    public Guid? DistrictId { get; set; }
    public string? ImageUrl { get; set; }
    public string? OfficialUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateUnescoRecordRequest
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int InscribedYear { get; set; }
    public Guid? DistrictId { get; set; }
    public string? ImageUrl { get; set; }
    public string? OfficialUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

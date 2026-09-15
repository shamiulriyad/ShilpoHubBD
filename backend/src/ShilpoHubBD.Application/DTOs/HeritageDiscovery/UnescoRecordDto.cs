namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class UnescoRecordDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int InscribedYear { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? ImageUrl { get; set; }
    public string? OfficialUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class UpdateCulturalEventRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid DistrictId { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class LocalCuisineDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? WhereToTry { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public Guid? HeritagePlaceId { get; set; }
    public string? HeritagePlaceName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

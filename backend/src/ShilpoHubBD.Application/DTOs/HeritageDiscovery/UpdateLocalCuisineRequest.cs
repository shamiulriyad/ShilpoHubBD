namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class UpdateLocalCuisineRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DistrictId { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public string? WhereToTry { get; set; }
    public string? ImageUrl { get; set; }
    public string? Kind { get; set; }
    public string? Ingredients { get; set; }
    public string? SourceUrl { get; set; }
    public bool IsNationwide { get; set; }
    public bool IsActive { get; set; }
}

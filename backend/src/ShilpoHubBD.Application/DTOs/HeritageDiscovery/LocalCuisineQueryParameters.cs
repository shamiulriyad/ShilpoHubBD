namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class LocalCuisineQueryParameters
{
    public string? Search { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

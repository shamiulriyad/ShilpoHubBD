using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class HeritagePlaceQueryParameters
{
    public string? Search { get; set; }
    public Guid? DistrictId { get; set; }
    public HeritagePlaceType? PlaceType { get; set; }
    public bool? IsFeatured { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

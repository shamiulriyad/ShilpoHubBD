using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class HeritageRouteQueryParameters
{
    public RouteStatus? Status { get; set; }
    public bool? IsRecommended { get; set; }
    public Guid? DistrictId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

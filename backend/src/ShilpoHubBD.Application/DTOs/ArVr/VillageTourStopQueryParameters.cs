namespace ShilpoHubBD.Application.DTOs.ArVr;

public class VillageTourStopQueryParameters
{
    public Guid? HeritagePlaceId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

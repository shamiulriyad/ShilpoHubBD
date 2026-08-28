namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class HeritageFestivalQueryParameters
{
    public Guid? DistrictId { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

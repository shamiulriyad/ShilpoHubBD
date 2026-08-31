namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class CreateHeritageFestivalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DistrictId { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsRecurringAnnually { get; set; }
    public string? ImageUrl { get; set; }
}

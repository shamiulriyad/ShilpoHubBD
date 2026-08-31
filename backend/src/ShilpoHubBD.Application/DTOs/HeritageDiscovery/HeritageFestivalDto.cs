namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class HeritageFestivalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsRecurringAnnually { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public Guid? HeritagePlaceId { get; set; }
    public string? HeritagePlaceName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

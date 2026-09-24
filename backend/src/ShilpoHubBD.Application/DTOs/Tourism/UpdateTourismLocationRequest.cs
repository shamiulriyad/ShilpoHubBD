using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.DTOs.Tourism;

public class UpdateTourismLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public TourismLocationType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid DistrictId { get; set; }
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal? Price { get; set; }
    public decimal? EntryFee { get; set; }
    public string? OpeningHours { get; set; }
    public string? ContactInfo { get; set; }
    public string? Facilities { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }
}

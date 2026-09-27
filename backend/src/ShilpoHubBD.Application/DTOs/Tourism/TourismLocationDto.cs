namespace ShilpoHubBD.Application.DTOs.Tourism;

public class TourismLocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal? Price { get; set; }
    public decimal? EntryFee { get; set; }
    public string? OpeningHours { get; set; }
    public string? ContactInfo { get; set; }
    public string? Facilities { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageCredit { get; set; }
    public string? ImageSourceUrl { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public string? Area { get; set; }
    public string? SourceUrl { get; set; }
    public string VerificationStatus { get; set; } = "Unverified";
    public string? UnverifiedFields { get; set; }
    public string? CoordinatesSource { get; set; }
    public string? CoordinatesPrecision { get; set; }
    public DateTime? DataRetrievedOn { get; set; }
    public string Source { get; set; } = "Admin";
    public string? ExternalId { get; set; }
    public string? Upazila { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

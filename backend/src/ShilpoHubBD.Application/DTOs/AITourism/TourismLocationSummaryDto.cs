namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourismLocationSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public decimal? EntryFee { get; set; }
    public bool IsVerified { get; set; }
    public string VerificationStatus { get; set; } = "Unverified";
    public string? Area { get; set; }
    public string? CoordinatesPrecision { get; set; }
    public string? OpeningHours { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string DistrictName { get; set; } = string.Empty;
}

namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageLocationRecordDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PlaceType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

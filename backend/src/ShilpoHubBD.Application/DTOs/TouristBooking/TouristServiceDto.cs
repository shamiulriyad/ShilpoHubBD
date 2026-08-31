namespace ShilpoHubBD.Application.DTOs.TouristBooking;

public class TouristServiceDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? DurationMinutes { get; set; }
    public int DefaultCapacity { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

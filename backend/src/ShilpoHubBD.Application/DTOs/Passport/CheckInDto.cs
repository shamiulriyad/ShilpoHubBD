namespace ShilpoHubBD.Application.DTOs.Passport;

public class CheckInDto
{
    public Guid Id { get; set; }
    public Guid HeritagePlaceId { get; set; }
    public string HeritagePlaceName { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CheckedInAt { get; set; }
}

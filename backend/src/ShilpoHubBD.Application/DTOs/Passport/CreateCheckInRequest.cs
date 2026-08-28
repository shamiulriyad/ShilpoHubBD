namespace ShilpoHubBD.Application.DTOs.Passport;

public class CreateCheckInRequest
{
    public Guid HeritagePlaceId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

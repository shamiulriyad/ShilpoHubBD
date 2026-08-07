namespace ShilpoHubBD.Application.DTOs.Community;

public class CreateVillageRequest
{
    public string Name { get; set; } = string.Empty;
    public string Craft { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid DistrictId { get; set; }
}

namespace ShilpoHubBD.Application.DTOs.Community;

public class VillageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Craft { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public bool IsFavorited { get; set; }
}

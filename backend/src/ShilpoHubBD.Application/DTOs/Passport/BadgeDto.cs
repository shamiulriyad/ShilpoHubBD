namespace ShilpoHubBD.Application.DTOs.Passport;

public class BadgeDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? FestivalName { get; set; }
    public int? RequiredPurchaseCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

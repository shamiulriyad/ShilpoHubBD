using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Application.DTOs.Passport;

public class CreateBadgeRequest
{
    public BadgeType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public Guid? DistrictId { get; set; }
    public string? FestivalName { get; set; }
    public int? RequiredPurchaseCount { get; set; }
    public int? RequiredCheckInCount { get; set; }
}

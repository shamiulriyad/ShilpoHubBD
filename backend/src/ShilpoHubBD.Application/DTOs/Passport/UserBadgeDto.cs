namespace ShilpoHubBD.Application.DTOs.Passport;

public class UserBadgeDto
{
    public Guid Id { get; set; }
    public Guid BadgeId { get; set; }
    public string BadgeName { get; set; } = string.Empty;
    public string BadgeType { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public DateTime EarnedAt { get; set; }
}

using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Passport;

public class Badge
{
    public Guid Id { get; set; }

    public BadgeType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }

    // Set when Type == District.
    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    // Set when Type == Festival.
    public string? FestivalName { get; set; }

    // Set when Type == Purchase.
    public int? RequiredPurchaseCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}

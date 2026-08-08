using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Passport;

public class UserBadge
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid BadgeId { get; set; }
    public Badge Badge { get; set; } = null!;

    public DateTime EarnedAt { get; set; }
}

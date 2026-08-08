using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.LiveShopping;

public class LiveEventReaction
{
    public Guid Id { get; set; }

    public Guid LiveEventId { get; set; }
    public LiveEvent LiveEvent { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

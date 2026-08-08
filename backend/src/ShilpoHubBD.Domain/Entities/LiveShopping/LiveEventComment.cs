using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.LiveShopping;

public class LiveEventComment
{
    public Guid Id { get; set; }

    public Guid LiveEventId { get; set; }
    public LiveEvent LiveEvent { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

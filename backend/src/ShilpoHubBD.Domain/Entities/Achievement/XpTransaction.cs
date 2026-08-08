using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Achievement;

public class XpTransaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

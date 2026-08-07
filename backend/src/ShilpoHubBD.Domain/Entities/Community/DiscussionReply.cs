using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Community;

public class DiscussionReply
{
    public Guid Id { get; set; }

    public Guid ThreadId { get; set; }
    public DiscussionThread Thread { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

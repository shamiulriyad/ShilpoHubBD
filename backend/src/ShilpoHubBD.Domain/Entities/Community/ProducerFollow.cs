using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Community;

public class ProducerFollow
{
    public Guid Id { get; set; }

    public Guid FollowerId { get; set; }
    public User Follower { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

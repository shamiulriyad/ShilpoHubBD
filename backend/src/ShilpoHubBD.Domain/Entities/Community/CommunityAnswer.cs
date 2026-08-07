using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Community;

public class CommunityAnswer
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }
    public CommunityQuestion Question { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

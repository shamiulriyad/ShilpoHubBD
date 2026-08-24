using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.LiveClass;

public class LiveClassParticipant
{
    public Guid Id { get; set; }

    public Guid LiveClassId { get; set; }
    public LiveClass LiveClass { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime RegisteredAt { get; set; }
}

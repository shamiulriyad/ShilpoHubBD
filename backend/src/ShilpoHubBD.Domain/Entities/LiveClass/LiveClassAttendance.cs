using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.LiveClass;

public class LiveClassAttendance
{
    public Guid Id { get; set; }

    public Guid LiveClassId { get; set; }
    public LiveClass LiveClass { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

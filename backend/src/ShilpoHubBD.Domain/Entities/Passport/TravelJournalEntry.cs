using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Passport;

public class TravelJournalEntry
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    public Guid? HeritagePlaceId { get; set; }
    public HeritagePlace? HeritagePlace { get; set; }

    public Guid? CheckInId { get; set; }
    public HeritageCheckIn? CheckIn { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Passport;

public class HeritageCheckIn
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid HeritagePlaceId { get; set; }
    public HeritagePlace HeritagePlace { get; set; } = null!;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateOnly CheckInDate { get; set; }
    public DateTime CheckedInAt { get; set; }
}

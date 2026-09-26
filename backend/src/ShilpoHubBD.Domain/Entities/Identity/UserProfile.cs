using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Identity;

/// <summary>
/// The real-world details every member gives the platform, kept apart from the login account
/// (email / password / display name). Every profile carries a national ID number and is checked by an
/// admin before it counts as approved.
/// </summary>
public class UserProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string LegalName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NidNumber { get; set; } = string.Empty;

    /// <summary>What the person is skilled in (producers: their craft). Used to filter producers.</summary>
    public string? Expertise { get; set; }

    public Guid? DistrictId { get; set; }
    public District? District { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string? About { get; set; }

    public UserProfileStatus Status { get; set; } = UserProfileStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

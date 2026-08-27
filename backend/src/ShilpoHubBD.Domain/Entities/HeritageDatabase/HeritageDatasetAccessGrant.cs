using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.HeritageDatabase;

/// <summary>Explicit researcher access to a Researcher/Restricted dataset.</summary>
public class HeritageDatasetAccessGrant
{
    public Guid Id { get; set; }

    public Guid HeritageDatasetId { get; set; }
    public HeritageDataset Dataset { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public HeritageDatasetAccessRole AccessRole { get; set; } = HeritageDatasetAccessRole.Viewer;

    public Guid GrantedByUserId { get; set; }
    public User GrantedBy { get; set; } = null!;

    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

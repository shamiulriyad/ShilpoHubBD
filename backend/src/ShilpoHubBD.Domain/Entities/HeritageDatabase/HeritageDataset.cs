using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.HeritageDatabase;

/// <summary>
/// A registered dataset in the National Heritage Database. Live datasets project over existing
/// platform entities (Producer, Product, Village, Heritage, Tourism); imported datasets carry
/// file/import metadata on each <see cref="HeritageDatasetVersion"/>. No raw rows are stored here.
/// </summary>
public class HeritageDataset
{
    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public HeritageDatasetCategory Category { get; set; } = HeritageDatasetCategory.Mixed;
    public HeritageDatasetStatus Status { get; set; } = HeritageDatasetStatus.Draft;
    public HeritageDatasetAccessLevel AccessLevel { get; set; } = HeritageDatasetAccessLevel.Researcher;
    public HeritageDatasetSourceType SourceType { get; set; } = HeritageDatasetSourceType.PlatformLive;

    public string? SourceOrganization { get; set; }
    public string? SourceReference { get; set; }
    public string? License { get; set; }
    public string? Tags { get; set; }

    /// <summary>When true the record count is recomputed from live platform data on refresh.</summary>
    public bool IsLive { get; set; } = true;

    public int RecordCount { get; set; }
    public int VersionCount { get; set; }

    /// <summary>When the underlying data last changed (import date or last live refresh).</summary>
    public DateTime? DataUpdatedAt { get; set; }
    public DateTime? LastRefreshedAt { get; set; }

    public Guid? CurrentVersionId { get; set; }
    public HeritageDatasetVersion? CurrentVersion { get; set; }

    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<HeritageDatasetVersion> Versions { get; set; } = new List<HeritageDatasetVersion>();
    public ICollection<HeritageDatasetAccessGrant> AccessGrants { get; set; } = new List<HeritageDatasetAccessGrant>();
    public ICollection<HeritageDatasetExport> Exports { get; set; } = new List<HeritageDatasetExport>();
}

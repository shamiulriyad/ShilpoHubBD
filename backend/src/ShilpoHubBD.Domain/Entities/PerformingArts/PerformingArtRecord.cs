using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.PerformingArts;

public enum PerformingArtCategory
{
    FolkMusic,
    FolkTheatre,
    TraditionalDance,
    OralPerformance,
    RegionalTradition,
}

/// <summary>Editorial lifecycle. Only <see cref="Published"/> records are visible to the public and to the AI assistant.</summary>
public enum PerformingArtStatus
{
    Draft,
    InReview,
    Published,
    Archived,
}

/// <summary>A bilingual (English/Bangla) heritage record for a performing-arts tradition — folk music, theatre, dance,
/// oral performance or a regional performance form. Content must be traceable to at least one source before publishing.</summary>
public class PerformingArtRecord
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;

    public string TitleEn { get; set; } = string.Empty;
    public string? TitleBn { get; set; }

    /// <summary>Comma-separated alternative names/spellings (either language).</summary>
    public string? AlternateNames { get; set; }
    public PerformingArtCategory Category { get; set; }

    public string SummaryEn { get; set; } = string.Empty;
    public string? SummaryBn { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionBn { get; set; }
    public string? RegionalContextEn { get; set; }
    public string? RegionalContextBn { get; set; }

    /// <summary>One of Bangladesh's eight administrative divisions, or null for a nationwide tradition.</summary>
    public string? Division { get; set; }
    public Guid? DistrictId { get; set; }
    public District? District { get; set; }
    public string? Upazila { get; set; }

    /// <summary>Comma-separated communities associated with the tradition.</summary>
    public string? Communities { get; set; }

    /// <summary>Named practitioners/tradition bearers — one per line; only as recorded in a cited source.</summary>
    public string? Artists { get; set; }

    /// <summary>Comma-separated instruments used in performance.</summary>
    public string? Instruments { get; set; }

    /// <summary>Present condition as recorded by the source (e.g. "Becoming less").</summary>
    public string? ViabilityNote { get; set; }

    /// <summary>Identifier in an external inventory, e.g. a Cultural Heritage Inventory of Bangladesh unique ID.</summary>
    public string? InventoryReference { get; set; }

    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    public PerformingArtStatus Status { get; set; } = PerformingArtStatus.Draft;
    public string? ReviewNote { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PerformingArtSource> Sources { get; set; } = new List<PerformingArtSource>();
    public ICollection<PerformingArtMedia> Media { get; set; } = new List<PerformingArtMedia>();
}

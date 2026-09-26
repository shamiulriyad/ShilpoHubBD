using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.ProductSearch;

/// <summary>Who last set the descriptive attributes of a product.</summary>
public static class AttributesSources
{
    public const string Producer = "Producer";
    public const string Admin = "Admin";
}

/// <summary>
/// Descriptive, search-oriented facts about a product, entered or confirmed by the producer. AI never writes here
/// directly: AI output lands in <see cref="ProductAttributeSuggestion"/> and only becomes final once the producer
/// confirms it.
/// </summary>
public class ProductAttributes
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public List<string> Tags { get; set; } = new();

    /// <summary>Bangla / romanised synonyms ("জামদানি", "shari").</summary>
    public List<string> Keywords { get; set; } = new();
    public List<string> Occasions { get; set; } = new();
    public List<string> Colors { get; set; } = new();

    public string? CraftTechnique { get; set; }

    /// <summary>Handmade / Handloom / Hand-finished.</summary>
    public string? ProductionMethod { get; set; }

    public string? DimensionsText { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightGrams { get; set; }

    public bool MadeToOrder { get; set; }
    public int? LeadTimeDays { get; set; }
    public string? CareInstructions { get; set; }

    public string Source { get; set; } = AttributesSources.Producer;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum SuggestionStatus
{
    Pending = 0,
    Confirmed = 1,
    Dismissed = 2,
}

/// <summary>
/// AI-generated attribute suggestion awaiting the producer's review. The producer edits and confirms it (the confirmed
/// values are saved as <see cref="ProductAttributes"/>) or dismisses it; nothing here is search-visible on its own.
/// </summary>
public class ProductAttributeSuggestion
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>The suggested attributes (product type slug, material slugs, tags, ...) as JSON.</summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>Which model produced it (audit).</summary>
    public string Model { get; set; } = string.Empty;

    public SuggestionStatus Status { get; set; } = SuggestionStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
}

public static class IndexStatuses
{
    public const string Dirty = "Dirty";
    public const string Synced = "Synced";
    public const string Failed = "Failed";
    public const string Deleted = "Deleted";
}

/// <summary>
/// Sync bookkeeping between PostgreSQL (source of truth) and the product vector collection. The primary key is the
/// product id with NO foreign key on purpose: when a product is deleted the row survives as <c>Deleted</c> so the
/// worker can remove the vectors.
/// </summary>
public class ProductIndexState
{
    public Guid ProductId { get; set; }

    public string Status { get; set; } = IndexStatuses.Dirty;

    /// <summary>Bumped on every change; an ack only clears Dirty when it acknowledges the current version.</summary>
    public int Version { get; set; }

    /// <summary>Hash of the text that was last embedded; unchanged text means a payload-only refresh.</summary>
    public string? SearchTextHash { get; set; }
    public string? EmbeddingModel { get; set; }
    public DateTime? IndexedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public DateTime UpdatedAt { get; set; }
}

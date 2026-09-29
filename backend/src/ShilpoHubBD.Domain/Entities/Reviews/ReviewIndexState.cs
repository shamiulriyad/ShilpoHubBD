using ShilpoHubBD.Domain.Entities.ProductSearch;

namespace ShilpoHubBD.Domain.Entities.Reviews;

/// <summary>
/// Sync bookkeeping between PostgreSQL (source of truth) and the review vector collection — same shape and the
/// same <see cref="IndexStatuses"/> vocabulary as <see cref="ProductIndexState"/>, kept as its own row/table (and,
/// on the Python side, its own Qdrant collection) so review vectors never mix with product search data. No
/// foreign key on purpose: a deleted review's row survives as <c>Deleted</c> so the worker can remove its vectors.
/// </summary>
public class ReviewIndexState
{
    public Guid ReviewId { get; set; }

    public string Status { get; set; } = IndexStatuses.Dirty;

    /// <summary>Bumped on every change; an ack only clears Dirty when it acknowledges the current version.</summary>
    public int Version { get; set; }

    /// <summary>Hash of the text that was last embedded; unchanged text means a payload-only refresh.</summary>
    public string? TextHash { get; set; }
    public string? EmbeddingModel { get; set; }
    public DateTime? IndexedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public DateTime UpdatedAt { get; set; }
}

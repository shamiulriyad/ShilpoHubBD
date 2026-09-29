namespace ShilpoHubBD.Domain.Entities.Reviews;

/// <summary>
/// A producer-facing reminder, created exactly once per warning-threshold crossing for a product (the calling
/// service only ever inserts one of these on the Normal→Warning transition, never per-review) — its own
/// existence is the dedupe record, consumed by the existing generic notification mechanism
/// (<c>ShilpoHubDbContext.Notifications.cs</c>) the same way <c>OrderComplaint</c> or <c>ExpertiseCertificate</c>
/// rows are. Deliberately does NOT end in "Event" so it is not skipped by that mechanism's blanket "*Event" rule.
/// </summary>
public class ProducerModerationWarning
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Guid ProducerId { get; set; }

    public ReviewComplaintType ComplaintType { get; set; }
    public int SimilarComplaintCount { get; set; }
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

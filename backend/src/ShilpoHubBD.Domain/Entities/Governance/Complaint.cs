using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// A complaint routed to the Government / NGO desk (counterfeit goods, fraud, heritage
/// misrepresentation, service issues …). Tracked from submission through triage to resolution, with
/// an update thread and an optional link to a monitoring flag.
/// </summary>
public class Complaint
{
    public Guid Id { get; set; }

    /// <summary>Short human-facing reference, unique.</summary>
    public string ReferenceCode { get; set; } = string.Empty;

    public ComplaintCategory Category { get; set; }
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Submitted;
    public ComplaintPriority Priority { get; set; } = ComplaintPriority.Medium;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // ---- Complainant (may be anonymous) --------------------------------
    public Guid? ComplainantUserId { get; set; }
    public User? ComplainantUser { get; set; }
    public string? ComplainantName { get; set; }
    public string? ComplainantContact { get; set; }

    // ---- Who / what it is against -----------------------------------
    public MonitoringSubjectType AgainstType { get; set; }
    public Guid? AgainstId { get; set; }
    public string? AgainstLabel { get; set; }

    public Guid? RelatedOrderId { get; set; }
    public Order? RelatedOrder { get; set; }

    public Guid? MonitoringFlagId { get; set; }
    public MonitoringFlag? MonitoringFlag { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public User? ResolvedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ComplaintUpdate> Updates { get; set; } = new List<ComplaintUpdate>();
}

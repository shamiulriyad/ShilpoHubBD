using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>A message / status change on a <see cref="Complaint"/>.</summary>
public class ComplaintUpdate
{
    public Guid Id { get; set; }

    public Guid ComplaintId { get; set; }
    public Complaint Complaint { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    /// <summary>Internal notes are hidden from the complainant.</summary>
    public bool IsInternal { get; set; }

    public ComplaintStatus? FromStatus { get; set; }
    public ComplaintStatus? ToStatus { get; set; }

    public Guid ActorUserId { get; set; }
    public User Actor { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

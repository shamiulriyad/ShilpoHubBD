using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Admin;

/// <summary>A user-submitted government-ID / business-document verification, reviewed by an admin.</summary>
public class IdentityVerificationRequest
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public IdentityVerificationType Type { get; set; }
    public IdentityVerificationStatus Status { get; set; } = IdentityVerificationStatus.Pending;

    public string DocumentNumber { get; set; } = string.Empty;
    public string FrontImageUrl { get; set; } = string.Empty;
    public string? BackImageUrl { get; set; }
    public string? SelfieImageUrl { get; set; }

    public string? ApplicantNote { get; set; }

    public DateTime SubmittedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

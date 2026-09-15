namespace ShilpoHubBD.Application.DTOs.Admin;

public class IdentityVerificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }

    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;
    public string FrontImageUrl { get; set; } = string.Empty;
    public string? BackImageUrl { get; set; }
    public string? SelfieImageUrl { get; set; }
    public string? ApplicantNote { get; set; }

    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByName { get; set; }
    public string? RejectionReason { get; set; }
}

public class SubmitIdentityVerificationRequest
{
    /// <summary>NationalId, Passport, TradeLicense, BusinessRegistration or Other.</summary>
    public string Type { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string FrontImageUrl { get; set; } = string.Empty;
    public string? BackImageUrl { get; set; }
    public string? SelfieImageUrl { get; set; }
    public string? ApplicantNote { get; set; }
}

public class RejectIdentityVerificationRequest
{
    public string RejectionReason { get; set; } = string.Empty;
}

public class IdentityVerificationQueryParameters
{
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? Search { get; set; }
    public Guid? UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

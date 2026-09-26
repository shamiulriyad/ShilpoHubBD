using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.DTOs.Profiles;

public class UserProfileDto
{
    public Guid? Id { get; set; }

    // Login account (read-only here; never part of the profile).
    public string LoginEmail { get; set; } = string.Empty;

    /// <summary>Uploaded profile photo (relative URL); null until the member adds one.</summary>
    public string? PhotoUrl { get; set; }

    public bool Exists { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NidNumber { get; set; } = string.Empty;
    public string? Expertise { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string? About { get; set; }

    public string Status { get; set; } = "NotSubmitted";
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool ExpertiseRequired { get; set; }
}

public class UpsertUserProfileRequest
{
    public string LegalName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NidNumber { get; set; } = string.Empty;
    public string? Expertise { get; set; }
    public Guid? DistrictId { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string? About { get; set; }
}

public class ReviewUserProfileRequest
{
    public string? Notes { get; set; }
}

public class UserProfileListItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LoginName { get; set; } = string.Empty;
    public string LoginEmail { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string LegalName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NidNumber { get; set; } = string.Empty;
    public string? Expertise { get; set; }
    public string? DistrictName { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserProfileQueryParameters
{
    public UserProfileStatus? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

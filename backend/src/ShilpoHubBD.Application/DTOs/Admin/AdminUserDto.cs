namespace ShilpoHubBD.Application.DTOs.Admin;

public class AdminUserListItemDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public string IdentityVerificationStatus { get; set; } = "None";
    public DateTime CreatedAt { get; set; }
}

public class AdminUserDetailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<IdentityVerificationDto> IdentityVerifications { get; set; } = new();
}

/// <summary>Government/NGO self-registration is disabled (see RoleNames.SelfRegisterableRoles); a SuperAdmin
/// creates these accounts directly instead, with an initial password the user can change after logging in.</summary>
public class CreateGovernmentNgoUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class AdminUserQueryParameters
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Domain.Entities.Admin;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Services.Admin;

internal static class AdminMappings
{
    public static AdminUserListItemDto ToListItemDto(this User u, string identityVerificationStatus) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FullName = u.FullName,
        IsActive = u.IsActive,
        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
        IdentityVerificationStatus = identityVerificationStatus,
        CreatedAt = u.CreatedAt,
    };

    public static AdminUserDetailDto ToDetailDto(this User u, List<IdentityVerificationDto> verifications) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FullName = u.FullName,
        IsActive = u.IsActive,
        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt,
        IdentityVerifications = verifications,
    };

    public static PermissionDto ToDto(this Permission p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Name = p.Name,
        Module = p.Module,
        Description = p.Description,
        CreatedAt = p.CreatedAt,
    };

    public static IdentityVerificationDto ToDto(this IdentityVerificationRequest r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        UserFullName = r.User?.FullName,
        UserEmail = r.User?.Email,
        Type = r.Type.ToString(),
        Status = r.Status.ToString(),
        DocumentNumber = r.DocumentNumber,
        FrontImageUrl = r.FrontImageUrl,
        BackImageUrl = r.BackImageUrl,
        SelfieImageUrl = r.SelfieImageUrl,
        ApplicantNote = r.ApplicantNote,
        SubmittedAt = r.SubmittedAt,
        ReviewedAt = r.ReviewedAt,
        ReviewedByName = r.ReviewedBy?.FullName,
        RejectionReason = r.RejectionReason,
    };
}
